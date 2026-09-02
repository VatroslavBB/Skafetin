namespace Skafetin.Shared.DTOs;

public class InventoryDetailsDto : InventoryDto
{
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? Note { get; set; }
    public InventorySummaryDto Summary { get; set; } = new();
}

