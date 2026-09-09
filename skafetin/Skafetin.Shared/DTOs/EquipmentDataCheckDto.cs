namespace Skafetin.Shared.DTOs;

public class EquipmentDataCheckDto
{
    public bool IsReady { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}
