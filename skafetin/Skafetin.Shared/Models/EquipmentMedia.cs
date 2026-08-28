namespace Skafetin.Shared.Models;

public class EquipmentMedia
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public string Title { get; set; } = string.Empty;
    public string MediaKind { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }

    public int? UploadedByEmployeeId { get; set; }
    public Employee? UploadedByEmployee { get; set; }
}
