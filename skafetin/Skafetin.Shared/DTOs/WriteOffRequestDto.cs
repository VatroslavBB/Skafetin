namespace Skafetin.Shared.DTOs;

public class WriteOffRequestDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string InventoryNumber { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int RequestedByEmployeeId { get; set; }
    public string RequestedByEmployeeFullName { get; set; } = string.Empty;
    public int WriteOffRequestStatusId { get; set; }
    public string WriteOffRequestStatusName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedByEmployeeId { get; set; }
    public string? ProcessedByEmployeeFullName { get; set; }
    public string? DecisionNote { get; set; }
    public DateTime? ExecutedAt { get; set; }
}

