using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Shared.DTOs;
using Skafetin.Shared.Models;
using Skafetin.Api.Data;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly SkafetinDbContext _context;

    public LocationsController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetLocations(
        [FromQuery] string? search,
        [FromQuery] int? locationTypeId,
        [FromQuery] bool? isActive)
    {
        var query = _context.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(loc => loc.Name.Contains(search)
                                    || loc.City.Contains(search)
                                    || loc.Address.Contains(search));

        if (locationTypeId.HasValue)
            query = query.Where(loc => loc.LocationTypeId == locationTypeId.Value);

        if (isActive.HasValue)
            query = query.Where(loc => loc.IsActive == isActive.Value);

        var result = await query
            .OrderBy(loc => loc.Name)
            .Select(loc => new LocationDto
            {
                Id = loc.Id,
                Name = loc.Name,
                Address = loc.Address,
                Zip = loc.Zip,
                City = loc.City,
                LocationTypeId = loc.LocationTypeId,
                LocationTypeName = loc.LocationType!.Name,
                IsActive = loc.IsActive
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetLocationsLookup()
    {
        var result = await _context.Locations
            .Where(loc => loc.IsActive)
            .OrderBy(loc => loc.Name)
            .Select(loc => new LookupDto
            {
                Id = loc.Id,
                Name = loc.Name
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetLocationById(int id)
    {
        var result = await _context.Locations
            .Where(loc => loc.Id == id)
            .Select(loc => new LocationDto
            {
                Id = loc.Id,
                Name = loc.Name,
                Address = loc.Address,
                Zip = loc.Zip,
                City = loc.City,
                LocationTypeId = loc.LocationTypeId,
                LocationTypeName = loc.LocationType!.Name,
                IsActive = loc.IsActive
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> CreateLocation(SaveLocationDto dto)
    {
        if (!await _context.LocationTypes.AnyAsync(type => type.Id == dto.LocationTypeId))
            return BadRequest(new ErrorResponseDto { Message = "Odabrana vrsta lokacije ne postoji." });

        var location = new Location
        {
            Name = dto.Name,
            Address = dto.Address,
            Zip = dto.Zip,
            City = dto.City,
            LocationTypeId = dto.LocationTypeId,
            IsActive = dto.IsActive
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var result = await _context.Locations
            .Where(loc => loc.Id == location.Id)
            .Select(loc => new LocationDto
            {
                Id = loc.Id,
                Name = loc.Name,
                Address = loc.Address,
                Zip = loc.Zip,
                City = loc.City,
                LocationTypeId = loc.LocationTypeId,
                LocationTypeName = loc.LocationType!.Name,
                IsActive = loc.IsActive
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLocation(int id, SaveLocationDto dto)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location is null)
            return NotFound();

        if (!await _context.LocationTypes.AnyAsync(type => type.Id == dto.LocationTypeId))
            return BadRequest(new ErrorResponseDto { Message = "Odabrana vrsta lokacije ne postoji." });

        location.Name = dto.Name;
        location.Address = dto.Address;
        location.Zip = dto.Zip;
        location.City = dto.City;
        location.LocationTypeId = dto.LocationTypeId;
        location.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location is null)
            return NotFound();

        var hasEmployees = await _context.Employees.AnyAsync(emp => emp.LocationId == id);
        var hasEquipment = await _context.Equipment.AnyAsync(eq => eq.LocationId == id);
        var hasInventories = await _context.Inventories.AnyAsync(inv => inv.LocationId == id);

        if (hasEmployees || hasEquipment || hasInventories)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Lokaciju nije moguće obrisati jer ima pridruženih zaposlenika, opreme ili inventura. Umjesto brisanja, označi je kao neaktivnu."
            });

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

