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
        if (activeOnly.HasValue)
        {
            query = query.Where(a => a.ReturnedAt == null);
        }
        if (from.HasValue)
        {
            query = query.Where(a => a.AssignedAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(a => a.AssignedAt <= to.Value);
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

    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<AssignmentDto>>> GetMyAssignments(bool? activeOnly)
    {

    }

    [HttpGet("equipment/{equipmentId:int}")]
    public async Task<ActionResult<List<AssignmentDto>>> GetAssignmentsWithEquipmentId(int id)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost]
    public async Task<ActionResult<AssignmentDto>> CreateAssignment(SaveAssignmentDto dto)
    {

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
}

