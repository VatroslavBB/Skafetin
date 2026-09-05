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
public class EquipmentRequestsController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    private const int RequestStatusReceived = 1;
    private const int RequestStatusInProgress = 2;
    private const int RequestStatusApproved = 3;
    private const int RequestStatusRejected = 4;
    private const int RequestStatusFulfilled = 5;
    private const int RequestStatusClosed = 6;

    private const int EquipmentStatusWrittenOff = 5;

    public EquipmentRequestsController(SkafetinDbContext context)
    {
        _context = context;
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpGet]
    public async Task<ActionResult<List<EquipmentRequestDto>>> GetEquipmentRequests(
        [FromQuery] string? search,
        [FromQuery] int? statusId,
        [FromQuery] int? categoryId,
        [FromQuery] int? employeeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.EquipmentRequests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r => EF.Functions.Like(r.Title, $"%{term}%")
                                  || EF.Functions.Like(r.Description, $"%{term}%")
                                  || EF.Functions.Like(r.RequestedByEmployee!.FirstName + " " + r.RequestedByEmployee!.LastName, $"%{term}%"));
        }
        if (statusId.HasValue)
        {
            query = query.Where(r => r.RequestStatusId == statusId.Value);
        }
        if (categoryId.HasValue)
        {
            query = query.Where(r => r.EquipmentCategoryId == categoryId.Value);
        }
        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequestedByEmployeeId == employeeId.Value);
        }
        if (from.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(r => r.CreatedAt < to.Value.Date.AddDays(1));
        }

        var result = await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Select(ToDto)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EquipmentRequestDto>> GetEquipmentRequestById(int id)
    {
        var request = await _context.EquipmentRequests
            .Where(r => r.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
        if (request is null)
            return NotFound();
        if (User.IsInRole("Admin") || User.IsInRole("AssetManager"))
            return Ok(request);

        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;
        if (!int.TryParse(claim, out var employeeId) || request.RequestedByEmployeeId != employeeId)
            return Forbid();

        return Ok(request);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<EquipmentRequestDto>>> GetMyEquipmentRequests([FromQuery] int? statusId)
    {
        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;
        if (!int.TryParse(claim, out var employeeId))
            return Forbid();

        var query = _context.EquipmentRequests
            .Where(r => r.RequestedByEmployeeId == employeeId);

        if (statusId.HasValue)
        {
            query = query.Where(r => r.RequestStatusId == statusId.Value);
        }

        var result = await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Select(ToDto)
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost("mine")]
    public async Task<ActionResult<EquipmentRequestDto>> CreateMyEquipmentRequest(SaveEquipmentRequestDto dto)
    {
        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;
        if (!int.TryParse(claim, out var employeeId))
            return Forbid();

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee is null)
            return Forbid();

        var categoryExists = await _context.EquipmentCategories.AnyAsync(c => c.Id == dto.EquipmentCategoryId);
        if (!categoryExists)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrana kategorija opreme ne postoji."
            });

        if (dto.ReplacementForEquipmentId.HasValue)
        {
            var replacementExists = await _context.Equipment
                .AnyAsync(e => e.Id == dto.ReplacementForEquipmentId.Value);
            if (!replacementExists)
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Oprema koja se zamjenjuje ne postoji."
                });
        }

        var request = new EquipmentRequest
        {
            RequestedByEmployeeId = employeeId,
            EquipmentCategoryId = dto.EquipmentCategoryId,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            ReplacementForEquipmentId = dto.ReplacementForEquipmentId,
            RequestStatusId = RequestStatusReceived,
            CreatedAt = DateTime.Now
        };

        _context.EquipmentRequests.Add(request);
        await _context.SaveChangesAsync();

        var result = await _context.EquipmentRequests
            .Where(r => r.Id == request.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetEquipmentRequestById), new { id = request.Id }, result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("{id:int}/process")]
    public async Task<ActionResult<EquipmentRequestDto>> ProcessEquipmentRequest(int id, ProcessRequestDto dto)
    {
        var request = await _context.EquipmentRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null)
            return NotFound();

        if (!IsAllowedTransition(request.RequestStatusId, dto.NewStatusId))
            return BadRequest(new ErrorResponseDto
            {
                Message = "Prijelaz u odabrani status nije dopušten."
            });
        if (dto.NewStatusId == RequestStatusRejected && string.IsNullOrWhiteSpace(dto.DecisionNote))
            return BadRequest(new ErrorResponseDto
            {
                Message = "Kod odbijanja zahtjeva obrazloženje je obavezno."
            });
        request.RequestStatusId = dto.NewStatusId;

        if (!string.IsNullOrWhiteSpace(dto.DecisionNote))
            request.DecisionNote = dto.DecisionNote.Trim();
        if (dto.NewStatusId == RequestStatusApproved || dto.NewStatusId == RequestStatusRejected)
        {
            request.ProcessedAt = DateTime.Now;
            request.ProcessedByEmployeeId = GetCurrentEmployeeId();
        }

        await _context.SaveChangesAsync();

        var result = await _context.EquipmentRequests
            .Where(r => r.Id == request.Id)
            .Select(ToDto)
            .FirstAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("{id:int}/fulfill")]
    public async Task<ActionResult<EquipmentRequestDto>> FulfillEquipmentRequest(int id, FulfillRequestDto dto)
    {
        var request = await _context.EquipmentRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null)
            return NotFound();

        if (request.RequestStatusId != RequestStatusApproved)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Realizirati se može samo odobreni zahtjev."
            });
        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == dto.EquipmentId);
        if (equipment is null)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrana oprema ne postoji."
            });
        if (equipment.EquipmentStatusId == EquipmentStatusWrittenOff)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrana oprema je otpisana."
            });
        request.ResultingEquipmentId = equipment.Id;
        request.RequestStatusId = RequestStatusFulfilled;

        await _context.SaveChangesAsync();

        var result = await _context.EquipmentRequests
            .Where(r => r.Id == request.Id)
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
            (RequestStatusReceived, RequestStatusInProgress) => true,
            (RequestStatusInProgress, RequestStatusApproved) => true,
            (RequestStatusInProgress, RequestStatusRejected) => true,
            (RequestStatusRejected, RequestStatusClosed) => true,
            (RequestStatusFulfilled, RequestStatusClosed) => true,
            _ => false
        };

    public static readonly Expression<Func<EquipmentRequest, EquipmentRequestDto>> ToDto = r => new EquipmentRequestDto
    {
        Id = r.Id,
        RequestedByEmployeeId = r.RequestedByEmployeeId,
        RequestedByEmployeeFullName = r.RequestedByEmployee!.FirstName + " " + r.RequestedByEmployee!.LastName,
        EquipmentCategoryId = r.EquipmentCategoryId,
        EquipmentCategoryName = r.EquipmentCategory!.Name,
        Title = r.Title,
        Description = r.Description,
        ReplacementForEquipmentId = r.ReplacementForEquipmentId,
        ReplacementForEquipmentInventoryNumber = r.ReplacementForEquipment!.InventoryNumber,
        ReplacementForEquipmentName = r.ReplacementForEquipment!.Name,
        RequestStatusId = r.RequestStatusId,
        RequestStatusName = r.RequestStatus!.Name,
        CreatedAt = r.CreatedAt,
        ProcessedAt = r.ProcessedAt,
        ProcessedByEmployeeId = r.ProcessedByEmployeeId,
        ProcessedByEmployeeFullName = r.ProcessedByEmployee!.FirstName + " " + r.ProcessedByEmployee!.LastName,
        DecisionNote = r.DecisionNote,
        ResultingEquipmentId = r.ResultingEquipmentId,
        ResultingEquipmentInventoryNumber = r.ResultingEquipment!.InventoryNumber,
        ResultingEquipmentName = r.ResultingEquipment!.Name
    };
}

