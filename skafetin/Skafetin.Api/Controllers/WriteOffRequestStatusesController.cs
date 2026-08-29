using Skafetin.Api.Data;
using Skafetin.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WriteOffRequestStatusesController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    public WriteOffRequestStatusesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetWriteOffRequestStatuses()
    {
        var result = await _context.WriteOffRequestStatuses
            .Select(status => new LookupDto
            {
                Id = status.Id,
                Name = status.Name
            })
            .ToListAsync();

        return Ok(result);
    }
}

