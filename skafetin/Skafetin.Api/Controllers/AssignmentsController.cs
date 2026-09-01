using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Api.Data;
using Skafetin.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Skafetin.Api.Security;

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
}

