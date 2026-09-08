using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Ai;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private const int InventoryStatusLocked = 5;

    private readonly SkafetinDbContext _context;
    private readonly IAiService _aiService;

    public AiController(SkafetinDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("inventory-summary/{inventoryId:int}")]
    public async Task<ActionResult<AiSuggestionDto>> GetInventorySummarySuggestion(
        int inventoryId,
        CancellationToken cancellationToken)
    {
        var inventory = await _context.Inventories
            .Where(i => i.Id == inventoryId)
            .Select(i => new
            {
                i.Code,
                LocationName = i.Location!.Name,
                IsLocked = i.InventoryStatusId == InventoryStatusLocked
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (inventory is null)
            return NotFound();
        var summary = await _context.Inventories
            .Where(i => i.Id == inventoryId)
            .Select(InventoryProjections.ToSummaryDto)
            .FirstAsync(cancellationToken);
        var suggestion = await _aiService.SummarizeInventoryAsync(
            new InventorySummaryContext(
                inventory.Code,
                inventory.LocationName,
                inventory.IsLocked,
                summary),
            cancellationToken);
        return Ok(suggestion);
    }

    [HttpPost("request-draft")]
    public async Task<ActionResult<AiSuggestionDto>> GetRequestDraft(
        RequestDraftDto dto,
        CancellationToken cancellationToken)
    {
        var categoryName = await _context.EquipmentCategories
            .Where(c => c.Id == dto.EquipmentCategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (categoryName is null)
            return BadRequest("Odabrana kategorija opreme ne postoji.");

        string? replacementName = null;

        if (dto.ReplacementForEquipmentId.HasValue)
        {
            replacementName = await _context.Equipment
                .Where(e => e.Id == dto.ReplacementForEquipmentId.Value)
                .Select(e => e.Name + " (" + e.InventoryNumber + ")")
                .FirstOrDefaultAsync(cancellationToken);

            if (replacementName is null)
                return BadRequest("Odabrana zamjenska oprema ne postoji.");
        }

        var suggestion = await _aiService.DraftRequestAsync(
            new RequestDraftContext(dto.Title.Trim(), categoryName, replacementName),
            cancellationToken);

        return Ok(suggestion);
    }
}


