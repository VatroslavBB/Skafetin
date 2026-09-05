using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Nova lozinka je obavezna.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati barem 8 znakova.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Lozinke se ne podudaraju.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
