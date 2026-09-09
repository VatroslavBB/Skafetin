namespace Skafetin.Shared.Models;

public class EquipmentStatusHistory
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public int? FromStatusId { get; set; }
    public EquipmentStatus? FromStatus { get; set; }

    public int ToStatusId { get; set; }
    public EquipmentStatus? ToStatus { get; set; }

    public int? FromLocationId { get; set; }
    public Location? FromLocation { get; set; }

    public int? ToLocationId { get; set; }
    public Location? ToLocation { get; set; }

    public DateTime ChangedAt { get; set; }

    public int? ChangedByEmployeeId { get; set; }
    public Employee? ChangedByEmployee { get; set; }

    public string? Reason { get; set; }
}
