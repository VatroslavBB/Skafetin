namespace Skafetin.Shared.DTOs;

public class InventorySummaryDto
{
    public int Total { get; set; }
    public int Counted { get; set; }
    public int Missing { get; set; }
    public int Damaged { get; set; }
    public int WrongLocation { get; set; }
}

