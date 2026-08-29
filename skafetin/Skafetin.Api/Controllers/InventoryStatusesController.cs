using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Shared.DTOs;
using Skafetin.Api.Data;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryStatusesController: ControllerBase
{
    private readonly SkafetinDbContext _context;

    public InventoryStatusesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetInventoryStatuses()
    {
        var result = await _context.InventoryStatuses
            .Select(status => new LookupDto
            {
                Id = status.Id,
                Name = status.Name
            })
            .ToListAsync();
        return Ok(result);
    }
}

