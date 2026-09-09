using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Shared.DTOs;
using Skafetin.Api.Data;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationTypesController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    public LocationTypesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetLocationTypes()
    {
        var result = await _context.LocationTypes
            .OrderBy(type => type.Name)
            .Select(type => new LookupDto
            {
                Id = type.Id,
                Name = type.Name
            })
            .ToListAsync();

        return Ok(result);
    }
}
