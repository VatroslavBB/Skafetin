namespace Skafetin.Shared.DTOs;

public class RecentActivityDto
{
    public DateTime OccurredAt { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? EquipmentId { get; set; }
}
