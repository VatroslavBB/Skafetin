using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveEquipmentRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite kategoriju opreme.")]
    public int EquipmentCategoryId { get; set; }

    [Required(ErrorMessage = "Naslov je obavezan.")]
    [StringLength(150, ErrorMessage = "Naslov smije imati najviše 150 znakova.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Obrazloženje je obavezno.")]
    [StringLength(1000, ErrorMessage = "Obrazloženje smije imati najviše 1000 znakova.")]
    public string Description { get; set; } = string.Empty;

    public int? ReplacementForEquipmentId { get; set; }
}
