using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveInventoryItemDto
{
    public bool IsFound { get; set; }

    public bool IsDamaged { get; set; }

    public int? FoundLocationId { get; set; }

    [StringLength(500, ErrorMessage = "Napomena je predugačka.")]
    public string? Note { get; set; }
}

