namespace Skafetin.Shared.Models;

public class InventoryItem
{
    public int Id { get; set; }

    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public int? ExpectedEmployeeId { get; set; }
    public Employee? ExpectedEmployee { get; set; }

    public int ExpectedLocationId { get; set; }
    public Location? ExpectedLocation { get; set; }

    public bool? IsFound { get; set; }
    public bool IsDamaged { get; set; }
    public int? FoundLocationId { get; set; }
    public Location? FoundLocation { get; set; }

    public string? Note { get; set; }
    public DateTime? CheckedAt { get; set; }

    public int? CheckedByEmployeeId { get; set; }
    public Employee? CheckedByEmployee { get; set; }
}
