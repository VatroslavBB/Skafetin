using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Shared.DTOs;
using Skafetin.Api.Data;

namespace Skafetin.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class EquipmentStatusesController: ControllerBase
{
    private readonly SkafetinDbContext _context;

    public EquipmentStatusesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetEquipmentStatus()
    {
        var result = await _context.EquipmentStatuses
            .Select(status => new LookupDto
            {
                Name = status.Name,
                Id = status.Id
            })
            .ToListAsync();
        return Ok(result);
    }
}

