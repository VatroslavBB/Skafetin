using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveEmployeeDto
{
    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(50, ErrorMessage = "Predugo ime.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(50, ErrorMessage = "Predugo prezime.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(100, ErrorMessage = "Predugo ime email adrese.")]
    [EmailAddress(ErrorMessage = "Potreban je validan format email adrese.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Predug telefonski broj.")]
    public string? Phone { get; set; }

    [StringLength(50, ErrorMessage = "Predugo ime radne pozicije.")]
    public string? JobTitle { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lokacija obavezna.")]
    public int LocationId { get; set; }

    public bool IsActive { get; set; } = true;
}

