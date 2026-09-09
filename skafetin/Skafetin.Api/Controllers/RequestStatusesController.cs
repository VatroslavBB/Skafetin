using Skafetin.Api.Data;
using Skafetin.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestStatusesController: ControllerBase
{
    private readonly SkafetinDbContext _context;

    public RequestStatusesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetRequestStatuses()
    {
        var result = await _context.RequestStatuses
            .Select(status => new LookupDto
            {
                Id = status.Id,
                Name = status.Name
            })
            .ToListAsync();
        return Ok(result);
    }
}

