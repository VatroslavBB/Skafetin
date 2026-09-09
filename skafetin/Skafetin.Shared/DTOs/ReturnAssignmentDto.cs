using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class ReturnAssignmentDto
{
    public DateTime ReturnedAt { get; set; }

    [StringLength(500, ErrorMessage = "Napomena je predugačka.")]
    public string? Note { get; set; }
}

