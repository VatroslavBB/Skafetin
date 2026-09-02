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

    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPost]
    public async Task<ActionResult<InventoryDto>> CreateInventory(SaveInventoryDto dto)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPost("{id:int}/open")]
    public async Task<ActionResult<InventoryDto>> OpenInventory(int id)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<InventoryDto>> CompleteInventory(int id)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("{id:int}/lock")]
    public async Task<ActionResult<InventoryDto>> LockInventory(int id)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetInventoryItems(
        [FromQuery] string? search,
        [FromQuery] bool? onlyDiscrepancies,
        [FromQuery] bool? isFound,
        [FromQuery] int? categoryId,
        [FromQuery] int? employeeId,
        int id)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpPut("{id:int}/items/{itemId:int}")]
    public async Task<ActionResult<InventoryItemDto>> UpdateInventoryItem(
        SaveInventoryItemDto dto,
        int id,
        int itemId)
    {

    }

    [Authorize(Policy = AuthorizationPolicies.InventoryWork)]
    [HttpGet("{id:int}/summary")]
    public async Task<ActionResult<InventorySummaryDto>> GetInventorySummary(int id)
    {

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
}

