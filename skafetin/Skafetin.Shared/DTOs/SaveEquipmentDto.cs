using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveEquipmentDto
{
    [Required(ErrorMessage = "Naziv obavezan.")]
    [StringLength(150, ErrorMessage = "Predugo ime.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Broj inventure je obavezan")]
    [StringLength(30, ErrorMessage = "Predug inventurni broj")]
    public string InventoryNumber { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Predug opis.")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Predugo ime proizvođača.")]
    public string? Manufacturer { get; set; }

    [StringLength(100, ErrorMessage = "Predugo ime modela.")]
    public string? Model { get; set; }

    [StringLength(100, ErrorMessage = "Predug serijski broj.")]
    public string? SerialNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Van definiranih granica.")]
    public int EquipmentCategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Van definiranih granica.")]
    public int EquipmentStatusId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Van definiranih granica.")]
    public int LocationId { get; set; }

    // Granice se parsiraju u invarijantnoj kulturi; na hrvatskom rasporedu je decimalni
    // razdjelnik zarez, pa bi ih atribut inace odbio i srusio validaciju forme.
    [Range(typeof(decimal), "0", "1000000",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Nabavna vrijednost mora biti između 0 i 1.000.000 EUR.")]
    public decimal? PurchaseValue { get; set; }
}