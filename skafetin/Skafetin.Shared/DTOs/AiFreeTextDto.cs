using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class AiFreeTextDto
{
    [Required(ErrorMessage = "Bilješka je obavezna.")]
    [StringLength(4000, ErrorMessage = "Bilješka smije imati najviše 4000 znakova.")]
    public string Text { get; set; } = string.Empty;
}
