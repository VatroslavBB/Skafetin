using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Ai;

public interface IAiService
{
    Task<AiSuggestionDto> SummarizeInventoryAsync(
        InventorySummaryContext context,
        CancellationToken cancellationToken = default);

    Task<AiSuggestionDto> DraftRequestAsync(
        RequestDraftContext context,
        CancellationToken cancellationToken = default);
}

public record InventorySummaryContext(
    string Code,
    string LocationName,
    bool IsLocked,
    InventorySummaryDto Summary);

public record RequestDraftContext(
    string Title,
    string CategoryName,
    string? ReplacementEquipmentName);
