namespace Skafetin.Shared.Models;

public class Equipment
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }

    public int EquipmentCategoryId { get; set; }
    public EquipmentCategory? EquipmentCategory { get; set; }

    public int EquipmentStatusId { get; set; }
    public EquipmentStatus? EquipmentStatus { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public decimal? PurchaseValue { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<EquipmentMedia> Media { get; set; } = new List<EquipmentMedia>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
