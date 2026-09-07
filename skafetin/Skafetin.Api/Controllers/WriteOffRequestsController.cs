using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Shared.DTOs;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WriteOffRequestsController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    private const int WriteOffStatusReceived = 1;
    private const int WriteOffStatusInProgress = 2;
    private const int WriteOffStatusApproved = 3;
    private const int WriteOffStatusRejected = 4;
    private const int WriteOffStatusExecuted = 5;

    private const int EquipmentStatusWriteOff = 5;

    private const int AssignmentStatusActive = 1;

    public WriteOffRequestsController(SkafetinDbContext context)
    {
        _context = context;
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpGet]
    public async Task<ActionResult<List<WriteOffRequestDto>>> GetWriteOffRequests(
        [FromQuery] string? search,
        [FromQuery] int? statusId,
        [FromQuery] int? equipmentId,
        [FromQuery] int? locationId)
    {
        var query = _context.WriteOffRequests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(w => EF.Functions.Like(w.Equipment!.Name, $"%{term}%")
                                  || EF.Functions.Like(w.Equipment!.InventoryNumber, $"%{term}%")
                                  || EF.Functions.Like(w.Reason, $"%{term}%")
                                  || EF.Functions.Like(w.RequestedByEmployee!.FirstName + " " + w.RequestedByEmployee!.LastName, $"%{term}%"));
        }
        if (statusId.HasValue)
        {
            query = query.Where(w => w.WriteOffRequestStatusId == statusId.Value);
        }
        if (equipmentId.HasValue)
        {
            query = query.Where(w => w.EquipmentId == equipmentId.Value);
        }
        if (locationId.HasValue)
        {
            query = query.Where(w => w.Equipment!.LocationId == locationId.Value);
        }

        var result = await query
            .OrderByDescending(w => w.CreatedAt)
            .ThenByDescending(w => w.Id)
            .Select(ToDto)
            .ToListAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WriteOffRequestDto>> GetWriteOffRequestById(int id)
    {
        var request = await _context.WriteOffRequests
            .Where(w => w.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
        if (request is null)
            return NotFound();

        return Ok(request);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPost]
    public async Task<ActionResult<WriteOffRequestDto>> CreateWriteOffRequest(SaveWriteOffRequestDto dto)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId is null)
            return Forbid();

        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == employeeId.Value);
        if (!employeeExists)
            return Forbid();

        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == dto.EquipmentId);
        if (equipment is null)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrana oprema ne postoji."
            });
        if (equipment.EquipmentStatusId == EquipmentStatusWriteOff)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrana oprema je već otpisana."
            });

        var openRequestExists = await _context.WriteOffRequests
            .AnyAsync(w => w.EquipmentId == dto.EquipmentId
                        && w.WriteOffRequestStatusId != WriteOffStatusRejected
                        && w.WriteOffRequestStatusId != WriteOffStatusExecuted);
        if (openRequestExists)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Za odabranu opremu već postoji zahtjev za otpisom u obradi."
            });

        var request = new WriteOffRequest
        {
            EquipmentId = equipment.Id,
            RequestedByEmployeeId = employeeId.Value,
            WriteOffRequestStatusId = WriteOffStatusReceived,
            Reason = dto.Reason.Trim(),
            CreatedAt = DateTime.Now
        };

        _context.WriteOffRequests.Add(request);
        await _context.SaveChangesAsync();

        var result = await _context.WriteOffRequests
            .Where(w => w.Id == request.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetWriteOffRequestById), new
        {
            id = request.Id
        }, result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("{id:int}/process")]
    public async Task<ActionResult<WriteOffRequestDto>> ProcessWriteOffRequest(int id, ProcessRequestDto dto)
    {
        var request = await _context.WriteOffRequests.FirstOrDefaultAsync(w => w.Id == id);
        if (request is null)
            return NotFound();

        if (!IsAllowedTransition(request.WriteOffRequestStatusId, dto.NewStatusId))
            return BadRequest(new ErrorResponseDto
            {
                Message = "Prijelaz u odabrani status nije dopušten."
            });
        if (dto.NewStatusId == WriteOffStatusRejected && string.IsNullOrWhiteSpace(dto.DecisionNote))
            return BadRequest(new ErrorResponseDto
            {
                Message = "Kod odbijanja zahtjeva obrazloženje je obavezno."
            });

        request.WriteOffRequestStatusId = dto.NewStatusId;

        if (!string.IsNullOrWhiteSpace(dto.DecisionNote))
            request.DecisionNote = dto.DecisionNote.Trim();
        if (dto.NewStatusId == WriteOffStatusApproved || dto.NewStatusId == WriteOffStatusRejected)
        {
            request.ProcessedAt = DateTime.Now;
            request.ProcessedByEmployeeId = GetCurrentEmployeeId();
        }

        await _context.SaveChangesAsync();

        var result = await _context.WriteOffRequests
            .Where(w => w.Id == request.Id)
            .Select(ToDto)
            .FirstAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:int}/execute")]
    public async Task<ActionResult<WriteOffRequestDto>> ExecuteWriteOffRequest(int id)
    {
        var request = await _context.WriteOffRequests.FirstOrDefaultAsync(w => w.Id == id);
        if (request is null)
            return NotFound();

        if (request.WriteOffRequestStatusId != WriteOffStatusApproved)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Provesti se može samo odobreni zahtjev za otpisom."
            });

        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == request.EquipmentId);
        if (equipment is null)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Oprema iz zahtjeva ne postoji."
            });

        var hasActiveAssignment = await _context.Assignments
            .AnyAsync(a => a.EquipmentId == request.EquipmentId
                        && a.AssignmentStatusId == AssignmentStatusActive);
        if (hasActiveAssignment)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Oprema je zadužena i mora se prvo vratiti."
            });
        equipment.EquipmentStatusId = EquipmentStatusWriteOff;
        request.WriteOffRequestStatusId = WriteOffStatusExecuted;
        request.ExecutedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        var result = await _context.WriteOffRequests
            .Where(w => w.Id == request.Id)
            .Select(ToDto)
            .FirstAsync();

        return Ok(result);
    }

    private int? GetCurrentEmployeeId()
    {
        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;
        return int.TryParse(claim, out var employeeId) ? employeeId : null;
    }

    private static bool IsAllowedTransition(int currentStatusId, int newStatusId) =>
        (currentStatusId, newStatusId) switch
        {
            (WriteOffStatusReceived, WriteOffStatusInProgress) => true,
            (WriteOffStatusInProgress, WriteOffStatusApproved) => true,
            (WriteOffStatusInProgress, WriteOffStatusRejected) => true,
            _ => false
        };

    public static readonly Expression<Func<WriteOffRequest, WriteOffRequestDto>> ToDto = w => new WriteOffRequestDto
    {
        Id = w.Id,
        EquipmentId = w.EquipmentId,
        EquipmentName = w.Equipment!.Name,
        InventoryNumber = w.Equipment!.InventoryNumber,
        LocationId = w.Equipment!.LocationId,
        LocationName = w.Equipment!.Location!.Name,
        RequestedByEmployeeId = w.RequestedByEmployeeId,
        RequestedByEmployeeFullName = w.RequestedByEmployee!.FirstName + " " + w.RequestedByEmployee!.LastName,
        WriteOffRequestStatusId = w.WriteOffRequestStatusId,
        WriteOffRequestStatusName = w.WriteOffRequestStatus!.Name,
        Reason = w.Reason,
        CreatedAt = w.CreatedAt,
        ProcessedAt = w.ProcessedAt,
        ProcessedByEmployeeId = w.ProcessedByEmployeeId,
        ProcessedByEmployeeFullName = w.ProcessedByEmployee!.FirstName + " " + w.ProcessedByEmployee!.LastName,
        DecisionNote = w.DecisionNote,
        ExecutedAt = w.ExecutedAt
    };
}

