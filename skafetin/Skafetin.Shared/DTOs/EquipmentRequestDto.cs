namespace Skafetin.Shared.DTOs;

public class EquipmentRequestDto
{
    public int Id { get; set; }

    public int RequestedByEmployeeId { get; set; }
    public string RequestedByEmployeeFullName { get; set; } = string.Empty;

    public int EquipmentCategoryId { get; set; }
    public string EquipmentCategoryName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? ReplacementForEquipmentId { get; set; }
    public string? ReplacementForEquipmentInventoryNumber { get; set; }
    public string? ReplacementForEquipmentName { get; set; }

    public int RequestStatusId { get; set; }
    public string RequestStatusName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedByEmployeeId { get; set; }
    public string? ProcessedByEmployeeFullName { get; set; }
    public string? DecisionNote { get; set; }

    public int? ResultingEquipmentId { get; set; }
    public string? ResultingEquipmentInventoryNumber { get; set; }
    public string? ResultingEquipmentName { get; set; }
}
