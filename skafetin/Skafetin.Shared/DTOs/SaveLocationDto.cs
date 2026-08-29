using System.ComponentModel.DataAnnotations;

namespace Skafetin.Shared.DTOs;

public class SaveLocationDto
{
    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(100, ErrorMessage = "Naziv može imati najviše 100 znakova.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresa je obavezna.")]
    [StringLength(200, ErrorMessage = "Adresa može imati najviše 200 znakova.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Poštanski broj je obavezan.")]
    [Range(10000, 99999, ErrorMessage = "Poštanski broj mora imati pet znamenki.")]
    public int Zip { get; set; }

    [Required(ErrorMessage = "Grad je obavezan.")]
    [StringLength(100, ErrorMessage = "Grad može imati najviše 100 znakova.")]
    public string City { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Vrsta lokacije je obavezna.")]
    public int LocationTypeId { get; set; }

    public bool IsActive { get; set; } = true;
}
