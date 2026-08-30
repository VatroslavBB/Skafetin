namespace Skafetin.Shared.DTOs;

public class EquipmentDto
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EquipmentCategoryId { get; set; }
    public int EquipmentStatusId { get; set; }
    public int LocationId { get; set; }
    public string EquipmentCategoryName { get; set; } = string.Empty;
    public string EquipmentStatusName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
}

