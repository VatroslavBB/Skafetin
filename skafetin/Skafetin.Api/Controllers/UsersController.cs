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
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UsersController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    public UsersController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserAdminDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int? roleId,
        [FromQuery] bool? isActive)
    {
        var query = _context.AppUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Username.Contains(search)
                                  || u.Email.Contains(search));

        if (roleId.HasValue)
            query = query.Where(u => u.UserRoles.Any(ur => ur.AppRoleId == roleId.Value));

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var users = await WithDetails(query)
            .OrderBy(u => u.Username)
            .ToListAsync();

        return Ok(users.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserAdminDto>> GetUserById(int id)
    {
        var user = await WithDetails(_context.AppUsers)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound();

        return Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserAdminDto>> CreateUser(SaveUserAdminDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new ErrorResponseDto { Message = "Lozinka je obavezna pri otvaranju računa." });

        var validationError = await ValidateAsync(dto, null);
        if (validationError is not null)
            return BadRequest(new ErrorResponseDto { Message = validationError });

        var (hash, salt) = PasswordHasher.Create(dto.Password);

        var user = new AppUser
        {
            Username = dto.Username.Trim(),
            Email = NormalizeEmail(dto.Email),
            PasswordHash = hash,
            PasswordSalt = salt,
            EmployeeId = dto.EmployeeId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var appRoleId in dto.RoleIds.Distinct())
            user.UserRoles.Add(new AppUserRole { AppRoleId = appRoleId });

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        var created = await WithDetails(_context.AppUsers)
            .FirstAsync(u => u.Id == user.Id);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, SaveUserAdminDto dto)
    {
        var user = await _context.AppUsers
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound();

        var validationError = await ValidateAsync(dto, id);
        if (validationError is not null)
            return BadRequest(new ErrorResponseDto { Message = validationError });

        var roleIds = dto.RoleIds.Distinct().ToList();
        if (IsCurrentUser(id))
        {
            var adminRoleId = await _context.AppRoles
                .Where(role => role.Name == "Admin")
                .Select(role => role.Id)
                .FirstOrDefaultAsync();

            if (!dto.IsActive || (adminRoleId != 0 && !roleIds.Contains(adminRoleId)))
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Ne možeš deaktivirati vlastiti račun ni ukloniti vlastitu Admin ulogu."
                });
        }

        user.Username = dto.Username.Trim();
        user.Email = NormalizeEmail(dto.Email);
        user.EmployeeId = dto.EmployeeId;
        user.IsActive = dto.IsActive;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var (hash, salt) = PasswordHasher.Create(dto.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        var toRemove = user.UserRoles.Where(ur => !roleIds.Contains(ur.AppRoleId)).ToList();
        foreach (var link in toRemove)
            user.UserRoles.Remove(link);

        var existingRoleIds = user.UserRoles.Select(ur => ur.AppRoleId).ToList();
        foreach (var appRoleId in roleIds.Where(rid => !existingRoleIds.Contains(rid)))
            user.UserRoles.Add(new AppUserRole { AppUserId = id, AppRoleId = appRoleId });

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto dto)
    {
        var user = await _context.AppUsers.FindAsync(id);

        if (user is null)
            return NotFound();

        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new ErrorResponseDto { Message = "Lozinke se ne podudaraju." });

        var (hash, salt) = PasswordHasher.Create(dto.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateAsync(SaveUserAdminDto dto, int? currentId)
    {
        var username = dto.Username.Trim();
        var email = NormalizeEmail(dto.Email);

        var id = currentId ?? 0;

        if (await _context.AppUsers.AnyAsync(u => u.Username == username && u.Id != id))
            return "Korisničko ime je već zauzeto.";

        if (await _context.AppUsers.AnyAsync(u => u.Email == email && u.Id != id))
            return "Račun s tom e-poštom već postoji.";

        if (dto.EmployeeId.HasValue)
        {
            if (!await _context.Employees.AnyAsync(emp => emp.Id == dto.EmployeeId.Value))
                return "Odabrani zaposlenik ne postoji.";

            if (await _context.AppUsers.AnyAsync(u => u.EmployeeId == dto.EmployeeId.Value && u.Id != id))
                return "Odabrani zaposlenik već ima korisnički račun.";
        }

        var roleIds = dto.RoleIds.Distinct().ToList();

        if (roleIds.Count == 0)
            return "Račun mora imati barem jednu ulogu.";

        var existingRoleCount = await _context.AppRoles.CountAsync(role => roleIds.Contains(role.Id));

        if (existingRoleCount != roleIds.Count)
            return "Jedna ili više odabranih uloga ne postoji.";

        return null;
    }

    private static IQueryable<AppUser> WithDetails(IQueryable<AppUser> query) => query
        .Include(u => u.Employee)
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.AppRole);

    private bool IsCurrentUser(int id)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(claim, out var currentUserId) && currentUserId == id;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static UserAdminDto ToDto(AppUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        EmployeeId = user.EmployeeId,
        EmployeeName = user.Employee == null
            ? null
            : user.Employee.FirstName + " " + user.Employee.LastName,
        RoleIds = user.UserRoles.Select(ur => ur.AppRoleId).ToList(),
        Roles = user.UserRoles.Select(ur => ur.AppRole!.Name).OrderBy(name => name).ToList()
    };
}

