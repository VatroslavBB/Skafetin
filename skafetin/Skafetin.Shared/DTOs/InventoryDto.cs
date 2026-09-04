namespace Skafetin.Shared.DTOs;

public class InventoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int InventoryStatusId { get; set; }
    public string InventoryStatusName { get; set; } = string.Empty;
    public int CreatedByEmployeeId { get; set; }
    public string CreatedByEmployeeFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

