using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Korisničko ime je obavezno.")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "Lozinka je obavezna.")]
    public string Password { get; set; } = string.Empty;
}

