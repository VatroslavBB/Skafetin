using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveUserAdminDto
{
    [Required(ErrorMessage = "Korisničko ime je obavezno.")]
    [StringLength(50, ErrorMessage = "Predugo korisničko ime.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email adresa je obavezna.")]
    [StringLength(150, ErrorMessage = "Predugo ime email adrese.")]
    [EmailAddress(ErrorMessage = "Potreban je validan format email adrese.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati barem 8 znakova.")]
    public string? Password { get; set; }

    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    [MinLength(1, ErrorMessage = "Odaberite barem jednu ulogu.")]
    public List<int> RoleIds { get; set; } = new();
}
