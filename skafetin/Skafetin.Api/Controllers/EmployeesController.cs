using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Shared.DTOs;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    public EmployeesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees(
        [FromQuery] string? search,
        [FromQuery] int? locationId,
        [FromQuery] bool? isActive)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(emp => emp.FirstName.Contains(search)
                                    || emp.LastName.Contains(search)
                                    || emp.Email.Contains(search));

        if (locationId.HasValue)
            query = query.Where(emp => emp.LocationId == locationId.Value);

        if (isActive.HasValue)
            query = query.Where(emp => emp.IsActive == isActive.Value);

        var result = await query
            .OrderBy(emp => emp.LastName)
            .ThenBy(emp => emp.FirstName)
            .Select(emp => new EmployeeDto
            {
                Id = emp.Id,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                FullName = emp.FirstName + " " + emp.LastName,
                Email = emp.Email,
                Phone = emp.Phone,
                JobTitle = emp.JobTitle,
                LocationId = emp.LocationId,
                LocationName = emp.Location!.Name,
                IsActive = emp.IsActive
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetEmployeesLookup([FromQuery] int? locationId)
    {
        var query = _context.Employees.Where(emp => emp.IsActive);

        if (locationId.HasValue)
            query = query.Where(emp => emp.LocationId == locationId.Value);

        var result = await query
            .OrderBy(emp => emp.LastName)
            .ThenBy(emp => emp.FirstName)
            .Select(emp => new LookupDto
            {
                Id = emp.Id,
                Name = emp.FirstName + " " + emp.LastName
            })
            .ToListAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeById(int id)
    {
        var result = await _context.Employees
            .Where(emp => emp.Id == id)
            .Select(emp => new EmployeeDto
            {
                Id = emp.Id,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                FullName = emp.FirstName + " " + emp.LastName,
                Email = emp.Email,
                Phone = emp.Phone,
                JobTitle = emp.JobTitle,
                LocationId = emp.LocationId,
                LocationName = emp.Location!.Name,
                IsActive = emp.IsActive
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(SaveEmployeeDto dto)
    {
        if (!await _context.Locations.AnyAsync(loc => loc.Id == dto.LocationId))
            return BadRequest(new ErrorResponseDto { Message = "Odabrana lokacija ne postoji." });

        if (await _context.Employees.AnyAsync(emp => emp.Email == dto.Email))
            return BadRequest(new ErrorResponseDto { Message = "Zaposlenik s tom e-poštom već postoji." });

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            JobTitle = dto.JobTitle,
            LocationId = dto.LocationId,
            IsActive = dto.IsActive
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var result = await _context.Employees
            .Where(emp => emp.Id == employee.Id)
            .Select(emp => new EmployeeDto
            {
                Id = emp.Id,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                FullName = emp.FirstName + " " + emp.LastName,
                Email = emp.Email,
                Phone = emp.Phone,
                JobTitle = emp.JobTitle,
                LocationId = emp.LocationId,
                LocationName = emp.Location!.Name,
                IsActive = emp.IsActive
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, result);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, SaveEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee is null)
            return NotFound();

        if (!await _context.Locations.AnyAsync(loc => loc.Id == dto.LocationId))
            return BadRequest(new ErrorResponseDto { Message = "Odabrana lokacija ne postoji." });

        if (await _context.Employees.AnyAsync(emp => emp.Email == dto.Email && emp.Id != id))
            return BadRequest(new ErrorResponseDto { Message = "Zaposlenik s tom e-poštom već postoji." });

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.JobTitle = dto.JobTitle;
        employee.LocationId = dto.LocationId;
        employee.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee is null)
            return NotFound();

        var hasAssignments = await _context.Assignments.AnyAsync(a => a.EmployeeId == id);
        var hasInventories = await _context.Inventories.AnyAsync(inv => inv.CreatedByEmployeeId == id);
        var hasRequests = await _context.EquipmentRequests.AnyAsync(r => r.RequestedByEmployeeId == id);
        var hasWriteOffs = await _context.WriteOffRequests.AnyAsync(w => w.RequestedByEmployeeId == id);
        var hasUserAccount = await _context.AppUsers.AnyAsync(u => u.EmployeeId == id);

        if (hasAssignments || hasInventories || hasRequests || hasWriteOffs || hasUserAccount)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Zaposlenika nije moguće obrisati jer ima zaduženja, inventure, zahtjeve ili korisnički račun. Umjesto brisanja, označi ga kao neaktivnog."
            });

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

