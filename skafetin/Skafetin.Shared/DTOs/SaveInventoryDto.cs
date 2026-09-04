using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveInventoryDto
{
    [Required(ErrorMessage = "Oznaka je obavezna.")]
    [StringLength(50, ErrorMessage = "Oznaka je predugačka.")]
    public string Code { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Lokacija nije odabrana.>")]
    public int LocationId { get; set; }

    [StringLength(500, ErrorMessage = "Napomena je predugačka.")]
    public string? Note { get; set; }
}

