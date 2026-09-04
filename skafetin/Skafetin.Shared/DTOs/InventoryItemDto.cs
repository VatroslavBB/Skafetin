namespace Skafetin.Shared.DTOs;

public class InventoryItemDto
{
    public int Id { get; set; }
    public int InventoryId { get; set; }

    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string InventoryNumber { get; set; } = string.Empty;
    public int EquipmentCategoryId { get; set; }
    public string EquipmentCategoryName { get; set; } = string.Empty;

    public int? ExpectedEmployeeId { get; set; }
    public string? ExpectedEmployeeFullName { get; set; }
    public int ExpectedLocationId { get; set; }
    public string ExpectedLocationName { get; set; } = string.Empty;

    public bool? IsFound { get; set; }
    public bool IsDamaged { get; set; }
    public int? FoundLocationId { get; set; }
    public string? FoundLocationName { get; set; }

    public string? Note { get; set; }
    public DateTime? CheckedAt { get; set; }
    public int? CheckedByEmployeeId { get; set; }
    public string? CheckedByEmployeeFullName { get; set; }
}

