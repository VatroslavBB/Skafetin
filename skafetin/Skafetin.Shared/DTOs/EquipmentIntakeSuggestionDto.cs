namespace Skafetin.Shared.DTOs;

public class EquipmentIntakeSuggestionDto
{
    public string Name { get; set; } = string.Empty;
    public string InventoryNumber { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public decimal? PurchaseValue { get; set; }

    public int EquipmentCategoryId { get; set; }
    public int LocationId { get; set; }

    public string? Description { get; set; }

    /// Procjena samog servisa, ne dokaz točnosti. Svako polje se i dalje provjerava.
    public double Confidence { get; set; }

    public List<string> Warnings { get; set; } = new();
}
