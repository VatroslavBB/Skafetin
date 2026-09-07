using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Skafetin.Api.Data;
using Skafetin.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Skafetin.Api.Security;
using System.Linq.Expressions;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoriesController: ControllerBase
{
    private readonly SkafetinDbContext _context;

    private const int InventoryStatusInit = 1;
    private const int InventoryStatusOpen = 2;
    private const int InventoryStatusInProgress = 3;
    private const int InventoryStatusCompleted = 4;
    private const int InventoryStatusLocked = 5;

    private const int EquipmentStatusWrittenOff = 5;
    private const int AssignmentStatusActive = 1;

    private const int DefaultItemsPageSize = 20;
    private const int MaxItemsPageSize = 100;

    public InventoriesController(SkafetinDbContext context)
    {
        _context = context;
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet]
    public async Task<ActionResult<List<InventoryDto>>> GetInventories(
        [FromQuery] string? search,
        [FromQuery] int? locationId,
        [FromQuery] int? statusId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.Inventories.AsQueryable();
        if(!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i => EF.Functions.Like(i.Code, $"%{term}%")
                                    || EF.Functions.Like(i.Location!.Name, $"%{term}%")
                                    || (i.Note != null && EF.Functions.Like(i.Note, $"%{term}%")));
        }
        if (locationId.HasValue)
            query = query.Where(i => i.LocationId == locationId.Value);

        var ownLocationId = GetRestrictedLocationId();
        if (ownLocationId.HasValue)
            query = query.Where(i => i.LocationId == ownLocationId.Value);

        if (statusId.HasValue)
            query = query.Where(i => i.InventoryStatusId == statusId.Value);
        if (from.HasValue)
            query = query.Where(i => i.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(i => i.CreatedAt < to.Value.Date.AddDays(1));

        var result = await query
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => i.Id)
           .Select(ToInventoryDto)
           .ToListAsync();
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InventoryDetailsDto>> GetInventoryById(int id)
    {
        var result = await _context.Inventories
            .Where(i => i.Id == id)
            .Select(ToInventoryDetailsDto)
            .FirstOrDefaultAsync();

        if (result is null)
            return NotFound();

        var ownLocationId = GetRestrictedLocationId();
        if (ownLocationId.HasValue && result.LocationId != ownLocationId.Value)
            return Forbid();

        result.Summary = await _context.Inventories
            .Where(i => i.Id == id)
            .Select(ToInventorySummaryDto)
            .FirstAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPost]
    public async Task<ActionResult<InventoryDto>> CreateInventory(SaveInventoryDto dto)
    {
        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;
        if (!int.TryParse(claim, out var employeeId))
            return Forbid();
        var ownLocationId = GetRestrictedLocationId();
        if (ownLocationId.HasValue && ownLocationId.Value != dto.LocationId)
            return Forbid();
        var locationExists = await _context.Locations.AnyAsync(l => l.Id == dto.LocationId);
        if (!locationExists)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Lokacija ne postoji."
            });
        var code = dto.Code.Trim();
        var codeExists = await _context.Inventories.AnyAsync(i => i.Code == code);
        if (codeExists)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Oznaka već postoji."
            });

        var inventory = new Inventory
        {
            Code = code,
            LocationId = dto.LocationId,
            InventoryStatusId = InventoryStatusInit,
            CreatedAt = DateTime.Now,
            Note = dto.Note,
            CreatedByEmployeeId = employeeId
        };
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        var result = await _context.Inventories
            .Where(i => i.Id == inventory.Id)
            .Select(ToInventoryDto)
            .FirstAsync();
        return CreatedAtAction(nameof(GetInventoryById), new
        {
            id = inventory.Id
        }, result);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPost("{id:int}/open")]
    public async Task<ActionResult<InventoryDto>> OpenInventory(int id)
    {
        var (inventory, error) = await LoadInventoryAsync(id);
        if (inventory is null)
            return error!;

        if (inventory.InventoryStatusId != InventoryStatusInit)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Inventura u krivom stanju"
            });
        var equipment = await _context.Equipment
            .Where(e => e.LocationId == inventory.LocationId
                     && e.EquipmentStatusId != EquipmentStatusWrittenOff)
            .Select(e => new { e.Id, e.LocationId })
            .ToListAsync();
        var activeAssignments = await _context.Assignments
            .Where(a => a.AssignmentStatusId == AssignmentStatusActive
                     && a.Equipment!.LocationId == inventory.LocationId)
            .Select(a => new { a.EquipmentId, a.EmployeeId })
            .ToDictionaryAsync(a => a.EquipmentId, a => a.EmployeeId);

        var items = equipment
            .Select(e => new InventoryItem
            {
                InventoryId = inventory.Id,
                EquipmentId = e.Id,
                ExpectedLocationId = e.LocationId,
                ExpectedEmployeeId = activeAssignments.TryGetValue(e.Id, out var expectedEmployeeId)
                    ? expectedEmployeeId
                    : null,
                IsFound = null,
                IsDamaged = false
            })
            .ToList();

        _context.InventoryItems.AddRange(items);

        inventory.InventoryStatusId = InventoryStatusOpen;
        inventory.StartedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        var result = await _context.Inventories
            .Where(i => i.Id == inventory.Id)
            .Select(ToInventoryDto)
            .FirstAsync();
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<InventoryDto>> CompleteInventory(int id)
    {
        var (inventory, error) = await LoadInventoryAsync(id);
        if (inventory is null)
            return error!;

        if (inventory.InventoryStatusId != InventoryStatusInProgress)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Inventura u krivom stanju."
            });

        inventory.InventoryStatusId = InventoryStatusCompleted;
        inventory.CompletedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        var result = await _context.Inventories
            .Where(i => i.Id == inventory.Id)
            .Select(ToInventoryDto)
            .FirstAsync();
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("{id:int}/lock")]
    public async Task<ActionResult<InventoryDto>> LockInventory(int id)
    {
        var (inventory, error) = await LoadInventoryAsync(id);
        if (inventory is null)
            return error!;

        if (inventory.InventoryStatusId != InventoryStatusCompleted)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Inventura u krivom stanju."
            });

        inventory.InventoryStatusId = InventoryStatusLocked;
        inventory.LockedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        var result = await _context.Inventories
            .Where(i => i.Id == inventory.Id)
            .Select(ToInventoryDto)
            .FirstAsync();
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<PagedResultDto<InventoryItemDto>>> GetInventoryItems(
        int id,
        [FromQuery] string? search,
        [FromQuery] bool? onlyDiscrepancies,
        [FromQuery] bool? isFound,
        [FromQuery] bool? isDamaged,
        [FromQuery] int? categoryId,
        [FromQuery] int? employeeId,
        [FromQuery] int? foundLocationId,
        [FromQuery] string? sortBy,
        [FromQuery] bool? sortDesc,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var (inventory, error) = await LoadInventoryAsync(id);
        if (inventory is null)
            return error!;

        var query = _context.InventoryItems.Where(x => x.InventoryId == id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.Like(x.Equipment!.Name, $"%{term}%")
                                  || EF.Functions.Like(x.Equipment!.InventoryNumber, $"%{term}%"));
        }

        if (onlyDiscrepancies == true)
            query = query.Where(x => x.IsFound == false
                                  || x.IsDamaged
                                  || (x.FoundLocationId != null
                                   && x.FoundLocationId != x.ExpectedLocationId));

        if (isFound.HasValue)
            query = query.Where(x => x.IsFound == isFound.Value);
        if (isDamaged.HasValue)
            query = query.Where(x => x.IsDamaged == isDamaged.Value);
        if (categoryId.HasValue)
            query = query.Where(x => x.Equipment!.EquipmentCategoryId == categoryId.Value);
        if (employeeId.HasValue)
            query = query.Where(x => x.ExpectedEmployeeId == employeeId.Value);
        if (foundLocationId.HasValue)
            query = query.Where(x => x.FoundLocationId == foundLocationId.Value);

        var descending = sortDesc ?? false;

        IOrderedQueryable<InventoryItem> ordered = sortBy?.ToLowerInvariant() switch
        {
            "inventorynumber" => descending
                ? query.OrderByDescending(x => x.Equipment!.InventoryNumber)
                : query.OrderBy(x => x.Equipment!.InventoryNumber),
            "category" => descending
                ? query.OrderByDescending(x => x.Equipment!.EquipmentCategory!.Name)
                : query.OrderBy(x => x.Equipment!.EquipmentCategory!.Name),
            "employee" => descending
                ? query.OrderByDescending(x => x.ExpectedEmployee!.LastName)
                : query.OrderBy(x => x.ExpectedEmployee!.LastName),
            "foundlocation" => descending
                ? query.OrderByDescending(x => x.FoundLocation!.Name)
                : query.OrderBy(x => x.FoundLocation!.Name),
            "isfound" => descending
                ? query.OrderByDescending(x => x.IsFound)
                : query.OrderBy(x => x.IsFound),
            "isdamaged" => descending
                ? query.OrderByDescending(x => x.IsDamaged)
                : query.OrderBy(x => x.IsDamaged),
            "checkedat" => descending
                ? query.OrderByDescending(x => x.CheckedAt)
                : query.OrderBy(x => x.CheckedAt),
            _ => descending
                ? query.OrderByDescending(x => x.Equipment!.Name)
                : query.OrderBy(x => x.Equipment!.Name)
        };

        var sorted = ordered.ThenBy(x => x.Id);

        var currentPage = page.HasValue && page.Value > 0 ? page.Value : 1;
        var currentPageSize = Math.Clamp(pageSize ?? DefaultItemsPageSize, 1, MaxItemsPageSize);

        var totalCount = await sorted.CountAsync();

        var items = await sorted
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(ToInventoryItemDto)
            .ToListAsync();

        return Ok(new PagedResultDto<InventoryItemDto>
        {
            TotalCount = totalCount,
            Page = currentPage,
            PageSize = currentPageSize,
            Items = items
        });
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPut("{id:int}/items/{itemId:int}")]
    public async Task<ActionResult<InventoryItemDto>> UpdateInventoryItem(
        int id,
        int itemId,
        SaveInventoryItemDto dto)
    {
        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;
        if (!int.TryParse(claim, out var employeeId))
            return Forbid();

        var (inventory, error) = await LoadInventoryAsync(id);
        if (inventory is null)
            return error!;
        if (inventory.InventoryStatusId == InventoryStatusLocked)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Inventura je zaključana."
            });

        if (inventory.InventoryStatusId != InventoryStatusOpen
            && inventory.InventoryStatusId != InventoryStatusInProgress)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Inventura nije otvorena."
            });

        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.InventoryId == id);

        if (item is null)
            return NotFound();

        if (dto.FoundLocationId.HasValue)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == dto.FoundLocationId.Value);
            if (!locationExists)
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Lokacija ne postoji."
                });
        }

        item.IsFound = dto.IsFound;
        item.IsDamaged = dto.IsDamaged;
        item.FoundLocationId = dto.IsFound ? dto.FoundLocationId : null;
        item.Note = dto.Note?.Trim();
        item.CheckedAt = DateTime.Now;
        item.CheckedByEmployeeId = employeeId;

        if (inventory.InventoryStatusId == InventoryStatusOpen)
            inventory.InventoryStatusId = InventoryStatusInProgress;

        await _context.SaveChangesAsync();

        var result = await _context.InventoryItems
            .Where(x => x.Id == item.Id)
            .Select(ToInventoryItemDto)
            .FirstAsync();
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet("{id:int}/summary")]
    public async Task<ActionResult<InventorySummaryDto>> GetInventorySummary(int id)
    {
        var (inventory, error) = await LoadInventoryAsync(id);
        if (inventory is null)
            return error!;

        var result = await _context.Inventories
            .Where(i => i.Id == id)
            .Select(ToInventorySummaryDto)
            .FirstAsync();
        return Ok(result);
    }

    private async Task<(Inventory? Inventory, ActionResult? Error)> LoadInventoryAsync(int id)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            return (null, NotFound());

        var ownLocationId = GetRestrictedLocationId();
        if (ownLocationId.HasValue && ownLocationId.Value != inventory.LocationId)
            return (null, Forbid());

        return (inventory, null);
    }

    private int? GetRestrictedLocationId()
    {
        if (User.IsInRole("Admin") || User.IsInRole("AssetManager"))
            return null;

        var claim = User.FindFirst(AppClaimTypes.LocationId)?.Value;

        return int.TryParse(claim, out var locationId) ? locationId : null;
    }

    private static readonly Expression<Func<Inventory, InventoryDto>> ToInventoryDto = i => new InventoryDto
    {
        Id = i.Id,
        Code = i.Code,
        LocationId = i.LocationId,
        LocationName = i.Location!.Name,
        InventoryStatusId = i.InventoryStatusId,
        InventoryStatusName = i.InventoryStatus!.Name,
        CreatedByEmployeeId = i.CreatedByEmployeeId,
        CreatedByEmployeeFullName = i.CreatedByEmployee!.FirstName + " " + i.CreatedByEmployee!.LastName,
        CreatedAt = i.CreatedAt
    };

    private static readonly Expression<Func<InventoryItem, InventoryItemDto>> ToInventoryItemDto = i => new InventoryItemDto
    {
        Id = i.Id,
        InventoryId = i.InventoryId,
        EquipmentId = i.EquipmentId,
        EquipmentName = i.Equipment!.Name,
        InventoryNumber = i.Equipment.InventoryNumber,
        EquipmentCategoryId = i.Equipment.EquipmentCategoryId,
        EquipmentCategoryName = i.Equipment.EquipmentCategory!.Name,
        ExpectedEmployeeId = i.ExpectedEmployeeId,
        ExpectedEmployeeFullName = i.ExpectedEmployee == null
            ? null
            : i.ExpectedEmployee.FirstName + " " + i.ExpectedEmployee.LastName,
        ExpectedLocationId = i.ExpectedLocationId,
        ExpectedLocationName = i.ExpectedLocation!.Name,
        IsFound = i.IsFound,
        IsDamaged = i.IsDamaged,
        FoundLocationId = i.FoundLocationId,
        FoundLocationName = i.FoundLocation == null ? null : i.FoundLocation.Name,
        Note = i.Note,
        CheckedAt = i.CheckedAt,
        CheckedByEmployeeId = i.CheckedByEmployeeId,
        CheckedByEmployeeFullName = i.CheckedByEmployee == null
            ? null
            : i.CheckedByEmployee.FirstName + " " + i.CheckedByEmployee.LastName
    };

    private static readonly Expression<Func<Inventory, InventorySummaryDto>> ToInventorySummaryDto = i => new InventorySummaryDto
    {
        Total = i.InventoryItems.Count,
        Counted = i.InventoryItems.Count(x => x.IsFound != null),
        Missing = i.InventoryItems.Count(x => x.IsFound == false),
        Damaged = i.InventoryItems.Count(x => x.IsFound == true && x.IsDamaged),
        WrongLocation = i.InventoryItems.Count(x => x.IsFound == true
                                                 && x.FoundLocationId != null
                                                 && x.FoundLocationId != x.ExpectedLocationId)
    };

    private static readonly Expression<Func<Inventory, InventoryDetailsDto>> ToInventoryDetailsDto = i => new InventoryDetailsDto
    {
        Id = i.Id,
        Code = i.Code,
        LocationId = i.LocationId,
        LocationName = i.Location!.Name,
        InventoryStatusId = i.InventoryStatusId,
        InventoryStatusName = i.InventoryStatus!.Name,
        CreatedByEmployeeId = i.CreatedByEmployeeId,
        CreatedByEmployeeFullName = i.CreatedByEmployee!.FirstName + " " + i.CreatedByEmployee!.LastName,
        CreatedAt = i.CreatedAt,
        StartedAt = i.StartedAt,
        CompletedAt = i.CompletedAt,
        LockedAt = i.LockedAt,
        Note = i.Note
    };
}

