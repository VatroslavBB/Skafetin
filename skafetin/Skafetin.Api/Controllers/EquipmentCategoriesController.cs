using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Api.Data;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentCategoriesController: ControllerBase
{
    private readonly SkafetinDbContext _context;

    public EquipmentCategoriesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetEquipmentCategories()
    {
        var result = await _context.EquipmentCategories
            .OrderBy(status => status.Name)
            .Select(status => new LookupDto
            {
                Name = status.Name,
                Id = status.Id
            })
            .ToListAsync();
        return Ok(result);
    }
}

