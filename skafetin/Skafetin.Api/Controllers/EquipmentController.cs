using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Shared.DTOs;
using Skafetin.Shared.Models;
using System.Linq.Expressions;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly SkafetinDbContext _context;

    public EquipmentController(SkafetinDbContext context)
    {
        _context = context;
    }

    private static readonly Expression<Func<Equipment, EquipmentDto>> ToDto = e => new EquipmentDto
    {
        Id = e.Id,
        InventoryNumber = e.InventoryNumber,
        Name = e.Name,
        Description = e.Description,
        EquipmentCategoryId = e.EquipmentCategoryId,
        EquipmentStatusId = e.EquipmentStatusId,
        LocationId = e.LocationId,
        EquipmentCategoryName = e.EquipmentCategory!.Name,
        EquipmentStatusName = e.EquipmentStatus!.Name,
        LocationName = e.Location!.Name
    };

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<EquipmentDto>>> GetEquipment(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? statusId,
        [FromQuery] int? locationId,
        [FromQuery] int? employeeId,
        [FromQuery] string? sortBy,
        [FromQuery] bool? sortDesc,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var query = _context.Equipment.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e => EF.Functions.Like(e.Name, $"%{term}%")
                                  || EF.Functions.Like(e.InventoryNumber, $"%{term}%"));
        }

        if (categoryId.HasValue)
            query = query.Where(e => e.EquipmentCategoryId == categoryId.Value);

        if (statusId.HasValue)
            query = query.Where(e => e.EquipmentStatusId == statusId.Value);

        if (locationId.HasValue)
            query = query.Where(e => e.LocationId == locationId.Value);

        if (employeeId.HasValue)
            query = query.Where(e => e.Assignments.Any(a => a.EmployeeId == employeeId.Value
                                                         && a.ReturnedAt == null));

        var descending = sortDesc ?? false;

        IOrderedQueryable<Equipment> ordered = sortBy?.ToLowerInvariant() switch
        {
            "inventorynumber" => descending
                ? query.OrderByDescending(e => e.InventoryNumber)
                : query.OrderBy(e => e.InventoryNumber),
            "category" => descending
                ? query.OrderByDescending(e => e.EquipmentCategory!.Name)
                : query.OrderBy(e => e.EquipmentCategory!.Name),
            "status" => descending
                ? query.OrderByDescending(e => e.EquipmentStatus!.Name)
                : query.OrderBy(e => e.EquipmentStatus!.Name),
            "location" => descending
                ? query.OrderByDescending(e => e.Location!.Name)
                : query.OrderBy(e => e.Location!.Name),
            "purchasevalue" => descending
                ? query.OrderByDescending(e => e.PurchaseValue)
                : query.OrderBy(e => e.PurchaseValue),
            "createdat" => descending
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt),
            _ => descending
                ? query.OrderByDescending(e => e.Name)
                : query.OrderBy(e => e.Name)
        };

        query = ordered.ThenBy(e => e.Id);

        var currentPage = page.HasValue && page.Value > 0 ? page.Value : 1;
        var currentPageSize = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(ToDto)
            .ToListAsync();

        return Ok(new PagedResultDto<EquipmentDto>
        {
            TotalCount = totalCount,
            Page = currentPage,
            PageSize = currentPageSize,
            Items = items
        });
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetEquipmentLookup([FromQuery] bool? availableOnly)
    {
        var query = _context.Equipment.AsQueryable();

        if (availableOnly == true)
            query = query.Where(e => e.EquipmentStatusId == 1);

        var result = await query
            .OrderBy(e => e.Name)
            .Select(e => new LookupDto
            {
                Id = e.Id,
                Name = e.InventoryNumber + " - " + e.Name
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EquipmentDetailsDto>> GetEquipmentById(int id)
    {
        var result = await _context.Equipment
            .Where(e => e.Id == id)
            .Select(e => new EquipmentDetailsDto
            {
                Id = e.Id,
                InventoryNumber = e.InventoryNumber,
                Name = e.Name,
                Description = e.Description,
                EquipmentCategoryId = e.EquipmentCategoryId,
                EquipmentStatusId = e.EquipmentStatusId,
                LocationId = e.LocationId,
                EquipmentCategoryName = e.EquipmentCategory!.Name,
                EquipmentStatusName = e.EquipmentStatus!.Name,
                LocationName = e.Location!.Name,
                Manufacturer = e.Manufacturer,
                Model = e.Model,
                PurchaseValue = e.PurchaseValue,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost]
    public async Task<ActionResult<EquipmentDto>> CreateEquipment(SaveEquipmentDto dto)
    {
        var lookupError = await ValidateLookupsAsync(dto);
        if (lookupError is not null)
            return BadRequest(new ErrorResponseDto { Message = lookupError });

        var inventoryNumber = dto.InventoryNumber.Trim();

        if (await _context.Equipment.AnyAsync(e => e.InventoryNumber == inventoryNumber))
            return BadRequest(new ErrorResponseDto
            {
                Message = $"Oprema s inventurnim brojem \"{inventoryNumber}\" već postoji."
            });

        var equipment = new Equipment
        {
            Name = dto.Name.Trim(),
            InventoryNumber = inventoryNumber,
            Description = dto.Description,
            Manufacturer = dto.Manufacturer,
            Model = dto.Model,
            EquipmentCategoryId = dto.EquipmentCategoryId,
            EquipmentStatusId = dto.EquipmentStatusId,
            LocationId = dto.LocationId,
            PurchaseValue = dto.PurchaseValue,
            CreatedAt = DateTime.Now
        };

        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();

        var result = await _context.Equipment
            .Where(e => e.Id == equipment.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetEquipmentById), new { id = equipment.Id }, result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEquipment(int id, SaveEquipmentDto dto)
    {
        var equipment = await _context.Equipment.FindAsync(id);

        if (equipment is null)
            return NotFound();

        var lookupError = await ValidateLookupsAsync(dto);
        if (lookupError is not null)
            return BadRequest(new ErrorResponseDto { Message = lookupError });

        var inventoryNumber = dto.InventoryNumber.Trim();

        if (await _context.Equipment.AnyAsync(e => e.InventoryNumber == inventoryNumber && e.Id != id))
            return BadRequest(new ErrorResponseDto
            {
                Message = $"Oprema s inventurnim brojem \"{inventoryNumber}\" već postoji."
            });

        equipment.Name = dto.Name.Trim();
        equipment.InventoryNumber = inventoryNumber;
        equipment.Description = dto.Description;
        equipment.Manufacturer = dto.Manufacturer;
        equipment.Model = dto.Model;
        equipment.EquipmentCategoryId = dto.EquipmentCategoryId;
        equipment.EquipmentStatusId = dto.EquipmentStatusId;
        equipment.LocationId = dto.LocationId;
        equipment.PurchaseValue = dto.PurchaseValue;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEquipment(int id)
    {
        var equipment = await _context.Equipment.FindAsync(id);

        if (equipment is null)
            return NotFound();

        var hasAssignments = await _context.Assignments.AnyAsync(a => a.EquipmentId == id);
        var hasInventoryItems = await _context.InventoryItems.AnyAsync(i => i.EquipmentId == id);
        var hasRequests = await _context.EquipmentRequests.AnyAsync(r => r.ReplacementForEquipmentId == id
                                                                     || r.ResultingEquipmentId == id);
        var hasWriteOffs = await _context.WriteOffRequests.AnyAsync(w => w.EquipmentId == id);
        var hasMedia = await _context.EquipmentMedia.AnyAsync(m => m.EquipmentId == id);
        var hasStatusHistory = await _context.EquipmentStatusHistories.AnyAsync(h => h.EquipmentId == id);

        if (hasAssignments || hasInventoryItems || hasRequests || hasWriteOffs || hasMedia || hasStatusHistory)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Opremu nije moguće obrisati jer ima povezanih zaduženja, stavki inventure, zahtjeva, otpisa ili dokumenata. Umjesto brisanja, promijeni joj status."
            });

        _context.Equipment.Remove(equipment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateLookupsAsync(SaveEquipmentDto dto)
    {
        if (!await _context.EquipmentCategories.AnyAsync(category => category.Id == dto.EquipmentCategoryId))
            return "Odabrana kategorija opreme ne postoji.";

        if (!await _context.EquipmentStatuses.AnyAsync(status => status.Id == dto.EquipmentStatusId))
            return "Odabrani status opreme ne postoji.";

        if (!await _context.Locations.AnyAsync(location => location.Id == dto.LocationId))
            return "Odabrana lokacija ne postoji.";

        return null;
    }
}

