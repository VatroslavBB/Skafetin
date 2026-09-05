using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class RolesController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    public RolesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetRolesLookup()
    {
        var result = await _context.AppRoles
            .OrderBy(role => role.Id)
            .Select(role => new LookupDto
            {
                Id = role.Id,
                Name = role.Name
            })
            .ToListAsync();

        return Ok(result);
    }
}

