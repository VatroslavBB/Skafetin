using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class FulfillRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite opremu.")]
    public int EquipmentId { get; set; }
}
