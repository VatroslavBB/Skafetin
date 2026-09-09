namespace Skafetin.Shared.Models;

public class Inventory
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public int InventoryStatusId { get; set; }
    public InventoryStatus? InventoryStatus { get; set; }

    public int CreatedByEmployeeId { get; set; }
    public Employee? CreatedByEmployee { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? LockedAt { get; set; }

    public string? Note { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
