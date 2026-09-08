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

    [HttpGet("status")]
    public ActionResult<AiProviderStatusDto> GetStatus()
    {
        return Ok(new AiProviderStatusDto
        {
            Provider = _aiService.ProviderName,
            Model = _aiService.ModelName,
            UsesExternalService = _aiService.UsesExternalService
        });
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

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("equipment-intake")]
    public async Task<ActionResult<EquipmentIntakeSuggestionDto>> SuggestEquipmentIntake(
        AiFreeTextDto dto,
        CancellationToken cancellationToken)
    {
        var categories = await _context.EquipmentCategories
            .OrderBy(c => c.Name)
            .Select(c => new LookupDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);

        var locations = await _context.Locations
            .OrderBy(l => l.Name)
            .Select(l => new LookupDto { Id = l.Id, Name = l.Name })
            .ToListAsync(cancellationToken);

        var suggestion = await _aiService.SuggestEquipmentIntakeAsync(
            new EquipmentIntakeContext(dto.Text.Trim(), categories, locations),
            cancellationToken);

        return Ok(suggestion);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("equipment-check")]
    public async Task<ActionResult<EquipmentDataCheckDto>> CheckEquipmentData(
        SaveEquipmentDto dto,
        CancellationToken cancellationToken)
    {
        var categoryName = await _context.EquipmentCategories
            .Where(c => c.Id == dto.EquipmentCategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var locationName = await _context.Locations
            .Where(l => l.Id == dto.LocationId)
            .Select(l => l.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var inventoryNumber = dto.InventoryNumber.Trim();

        var inventoryNumberTaken = !string.IsNullOrWhiteSpace(inventoryNumber)
            && await _context.Equipment.AnyAsync(e => e.InventoryNumber == inventoryNumber, cancellationToken);

        var serialNumber = dto.SerialNumber?.Trim();

        var serialNumberTaken = !string.IsNullOrWhiteSpace(serialNumber)
            && await _context.Equipment.AnyAsync(e => e.SerialNumber == serialNumber, cancellationToken);

        var result = await _aiService.CheckEquipmentDataAsync(
            new EquipmentCheckContext(dto, categoryName, locationName, inventoryNumberTaken, serialNumberTaken),
            cancellationToken);

        return Ok(result);
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


