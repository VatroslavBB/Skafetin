namespace Skafetin.Shared.DTOs;

public class EquipmentDetailsDto: EquipmentDto
{
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public decimal? PurchaseValue { get; set; }
    public DateTime CreatedAt { get; set; }
}

