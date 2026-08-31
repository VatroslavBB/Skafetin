using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class CancelAssignmentDto
{
    [Required(ErrorMessage = "Razlog je obavezan.")]
    [StringLength(100, ErrorMessage = "Razlog je predugačak.")]
    public string Reason { get; set; } = string.Empty;
}

