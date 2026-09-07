using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveWriteOffRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite opremu.")]
    public int EquipmentId { get; set; }

    [Required(ErrorMessage = "Obrazloženje je obavezno.")]
    [StringLength(1000, ErrorMessage = "Obrazloženje je predugačko.")]
    public string Reason { get; set; } = string.Empty;
}
