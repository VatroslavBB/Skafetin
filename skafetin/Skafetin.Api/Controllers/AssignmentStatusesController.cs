using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentStatusesController: ControllerBase
{
    private readonly SkafetinDbContext _context;

    public AssignmentStatusesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetAssignmentStatuses()
    {
        var result = await _context.AssignmentStatuses
            .Select(status => new LookupDto
                {
                Name = status.Name,
                Id = status.Id
                })
            .ToListAsync();
        return Ok(result);
    }
}

