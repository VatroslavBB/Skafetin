using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Shared.DTOs;
using Skafetin.Shared.Models;
using System.Security.Claims;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SkafetinDbContext _context;
    private readonly JwtTokenService _tokenService;

    public AuthController(SkafetinDbContext context, JwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var username = dto.Username.Trim();

        var user = await _context.AppUsers
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.AppRole)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user is null || !user.IsActive)
            return Unauthorized(new ErrorResponseDto { Message = "Pogrešno korisničko ime ili lozinka." });

        if (!PasswordHasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized(new ErrorResponseDto { Message = "Pogrešno korisničko ime ili lozinka." });

        var roles = user.UserRoles
            .Where(ur => ur.AppRole is not null)
            .Select(ur => ur.AppRole!.Name)
            .OrderBy(name => name)
            .ToList();

        var locationId = user.Employee?.LocationId;
        var (token, expiresAtUtc) = _tokenService.CreateToken(user, roles, locationId);

        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            User = ToLoggedUserDto(user, roles, locationId)
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<LoggedUserDto>> Me()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var userId))
            return Unauthorized();

        var user = await _context.AppUsers
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.AppRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null || !user.IsActive)
            return Unauthorized();

        var roles = user.UserRoles
            .Where(ur => ur.AppRole is not null)
            .Select(ur => ur.AppRole!.Name)
            .OrderBy(name => name)
            .ToList();

        return Ok(ToLoggedUserDto(user, roles, user.Employee?.LocationId));
    }

    private static LoggedUserDto ToLoggedUserDto(AppUser user, List<string> roles, int? locationId) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.Employee is null
            ? user.Username
            : $"{user.Employee.FirstName} {user.Employee.LastName}",
        Roles = roles,
        EmployeeId = user.EmployeeId,
        LocationId = locationId
    };
}

