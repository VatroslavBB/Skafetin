using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class MoveEquipmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odabir lokacije je obavezan.")]
    public int ToLocationId { get; set; }

    [StringLength(500, ErrorMessage = "Razlog je predugačak.")]
    public string? Reason { get; set; }
}
