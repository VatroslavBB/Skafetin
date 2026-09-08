using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Ai;

public interface IAiService
{
    string ProviderName { get; }
    string ModelName { get; }
    bool UsesExternalService { get; }

    Task<AiSuggestionDto> SummarizeInventoryAsync(
        InventorySummaryContext context,
        CancellationToken cancellationToken = default);

    Task<EquipmentIntakeSuggestionDto> SuggestEquipmentIntakeAsync(
        EquipmentIntakeContext context,
        CancellationToken cancellationToken = default);

    Task<EquipmentDataCheckDto> CheckEquipmentDataAsync(
        EquipmentCheckContext context,
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

public record EquipmentIntakeContext(
    string Text,
    IReadOnlyList<LookupDto> Categories,
    IReadOnlyList<LookupDto> Locations);

public record EquipmentCheckContext(
    SaveEquipmentDto Equipment,
    string? CategoryName,
    string? LocationName,
    bool InventoryNumberTaken,
    bool SerialNumberTaken);
