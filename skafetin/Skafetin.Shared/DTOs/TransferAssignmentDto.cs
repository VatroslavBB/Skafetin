using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class TransferAssignmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite zaposlenika.")]
    public int ToEmployeeId { get; set; }

    public DateTime TransferredAt { get; set; }

    [StringLength(500, ErrorMessage = "Napomena je predugačka.")]
    public string? Note { get; set; }
}

