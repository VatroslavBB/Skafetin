using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class RequestDraftDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite kategoriju opreme.")]
    public int EquipmentCategoryId { get; set; }

    [Required(ErrorMessage = "Naslov je obavezan.")]
    [StringLength(150, ErrorMessage = "Naslov smije imati najviše 150 znakova.")]
    public string Title { get; set; } = string.Empty;

    public int? ReplacementForEquipmentId { get; set; }
}
