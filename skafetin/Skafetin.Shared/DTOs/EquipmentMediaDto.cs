namespace Skafetin.Shared.DTOs;

public class EquipmentMediaDto
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string InventoryNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string MediaKind { get; set; } = string.Empty;
    public bool IsCover { get; set; }

    // Naziv koji je stigao od korisnika - sluzi samo za prikaz i preuzimanje.
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    // Ruta preko koje se dohvaca sadrzaj; ime datoteke na disku ostaje na serveru.
    public string ContentUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
    public int? UploadedByEmployeeId { get; set; }
    public string? UploadedByEmployeeFullName { get; set; }
}
