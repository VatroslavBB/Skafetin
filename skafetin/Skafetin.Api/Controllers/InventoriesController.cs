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
}

