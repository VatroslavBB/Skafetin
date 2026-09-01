using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Api.Data;
using Skafetin.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Skafetin.Api.Security;
using System.Linq.Expressions;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController: ControllerBase
{
    private readonly SkafetinDbContext _context;
    private const int AssignmentStatusActive = 1;
    private const int EquipmentStatusInStock = 1;
    private const int EquipmentStatusAssigned = 2;
    private const int EquipmentStatusWriteOff = 5;

    public AssignmentsController(SkafetinDbContext context)
    {
        _context = context;
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet]
    public async Task<ActionResult<List<AssignmentDto>>> GetAssignments(
        [FromQuery] string? search,
        [FromQuery] int? equipmentId,
        [FromQuery] int? employeeId,
        [FromQuery] int? statusId,
        [FromQuery] bool? activeOnly,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.Assignments.AsQueryable();
        if(!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a => EF.Functions.Like(a.Equipment!.InventoryNumber, $"%{term}%")
                                    || EF.Functions.Like(a.Equipment!.Name, $"%{term}%")
                                    || EF.Functions.Like(a.Employee!.FirstName + " " + a.Employee!.LastName, $"%{term}%"));
        }
        if (equipmentId.HasValue)
        {
            query = query.Where(a => a.EquipmentId == equipmentId.Value);
        }
        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }
        if (statusId.HasValue)
        {
            query = query.Where(a => a.AssignmentStatusId == statusId.Value);
        }
        if (activeOnly == true)
        {
            query = query.Where(a => a.ReturnedAt == null);
        }
        if (from.HasValue)
        {
            query = query.Where(a => a.AssignedAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(a => a.AssignedAt < to.Value.Date.AddDays(1));
        }

        var result = await query
            .OrderByDescending(a => a.AssignedAt)
            .ThenByDescending(a => a.Id)
            .Select(ToDto)
            .ToListAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentDto>> GetAssignmentById(int id)
    {
        var result = await _context.Assignments
            .Where(a => a.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<AssignmentDto>>> GetMyAssignments(bool? activeOnly)
    {
        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;
        if (!int.TryParse(claim, out var employeeId))
            return Forbid();
        var query = _context.Assignments
            .Where(a => a.EmployeeId == employeeId);
        if (activeOnly == true)
            query = query.Where(a => a.ReturnedAt == null);
        var result = await query.
            OrderByDescending(a => a.AssignedAt)
            .ThenByDescending(a => a.Id)
            .Select(ToDto)
            .ToListAsync();
        return Ok(result);
    }

    [HttpGet("equipment/{equipmentId:int}")]
    public async Task<ActionResult<List<AssignmentDto>>> GetAssignmentsWithEquipmentId(int id)
    {
        var exists = await _context.Equipment.AnyAsync(e => e.Id == id);
        if (!exists)
            return NotFound();
        var result = await _context.Assignments
            .Where(a => a.EquipmentId == id)
            .OrderByDescending(a => a.AssignedAt)
            .ThenByDescending(a => a.Id)
            .Select(ToDto)
            .ToListAsync();
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost]
    public async Task<ActionResult<AssignmentDto>> CreateAssignment(SaveAssignmentDto dto)
    {
        if (dto.AssignedAt > DateTime.Now)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Datum zaduženja pogrešan."
            });
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
                Message = "Odabrana oprema je otpisana."
            });
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
        if (employee is null)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrani zaposlenik ne postoji."
            });
        var equipmentInUse = await _context.Assignments
            .AnyAsync(a => a.EquipmentId == dto.EquipmentId);
        if (equipmentInUse)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrana oprema se već koristi."
            });

        var assignment = new Assignment
        {
            EquipmentId = dto.EquipmentId,
            EmployeeId = dto.EmployeeId,
            AssignedAt = dto.AssignedAt,
            AssignmentStatusId = AssignmentStatusActive,
            Note = dto.Note?.Trim(),
            CreatedAt = DateTime.Now
        };

        _context.Assignments.Add(assignment);
        equipment.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        var result = await _context.Assignments
            .Where(a => a.Id == assignment.Id)
            .Select(ToDto)
            .FirstAsync();
        return CreatedAtAction(nameof(GetAssignmentById), new
        {
            id = assignment.Id
        }, result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<AssignmentDto>> ReturnAssignment(ReturnAssignmentDto dto, int id)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpGet("{id:int}/transfer")]
    public async Task<ActionResult<AssignmentDto>> TransferAssignment(TransferAssignmentDto dto, int id)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpGet("{id:int}/cancel")]
    public async Task<IActionResult> CancelAssignment(CancelAssignmentDto dto, int id)
    {

    }

    public static readonly Expression<Func<Assignment, AssignmentDto>> ToDto = a => new AssignmentDto
    {
        Id = a.Id,
        EquipmentId = a.EquipmentId,
        InventoryNumber = a.Equipment!.InventoryNumber,
        EquipmentName = a.Equipment!.Name,
        EmployeeId = a.EmployeeId,
        EmployeeFullName = a.Employee!.FirstName + " " + a.Employee!.LastName,
        AssignedAt = a.AssignedAt,
        ReturnedAt = a.ReturnedAt,
        AssignmentStatusId = a.AssignmentStatusId,
        AssignmentStatusName = a.AssignmentStatus!.Name,
        PreviousAssignmentId = a.PreviousAssignmentId,
        Note = a.Note,
        CreatedAt = a.CreatedAt
    };

    public async Task<string?> ValidateLookupAsync(SaveAssignmentDto dto)
    {
        var equipmentIdOk = await _context.EquipmentCategories
            .AnyAsync(e => e.Id == dto.EquipmentId);
        var employeeIdOk = await _context.Employees
            .AnyAsync(e => e.Id == dto.EmployeeId);
        if (equipmentIdOk && employeeIdOk)
            return null;
        return "Zaposlenik ili oprema ne postoje.";
    }
}

