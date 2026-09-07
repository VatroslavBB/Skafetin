using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class ProcessRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite novi status.")]
    public int NewStatusId { get; set; }

    [StringLength(500, ErrorMessage = "Obrazloženje smije imati najviše 500 znakova.")]
    public string? DecisionNote { get; set; }
}
