namespace Skafetin.Shared.Models;

public class EquipmentRequest
{
    public int Id { get; set; }

    public int RequestedByEmployeeId { get; set; }
    public Employee? RequestedByEmployee { get; set; }

    public int EquipmentCategoryId { get; set; }
    public EquipmentCategory? EquipmentCategory { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? ReplacementForEquipmentId { get; set; }
    public Equipment? ReplacementForEquipment { get; set; }

    public int RequestStatusId { get; set; }
    public RequestStatus? RequestStatus { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public int? ProcessedByEmployeeId { get; set; }
    public Employee? ProcessedByEmployee { get; set; }

    public string? DecisionNote { get; set; }

    public int? ResultingEquipmentId { get; set; }
    public Equipment? ResultingEquipment { get; set; }
}
