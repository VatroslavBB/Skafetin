using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveAssignmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite opremu.")]
    public int EquipmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite zaposlenika.")]
    public int EmployeeId { get; set; }

    public DateTime AssignedAt { get; set; }

    [StringLength(500, ErrorMessage = "Napomena je predugačka.")]
    public string? Note { get; set; }
}

