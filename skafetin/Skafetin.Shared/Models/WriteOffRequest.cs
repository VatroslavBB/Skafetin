namespace Skafetin.Shared.Models;

public class WriteOffRequest
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public int RequestedByEmployeeId { get; set; }
    public Employee? RequestedByEmployee { get; set; }

    public int WriteOffRequestStatusId { get; set; }
    public WriteOffRequestStatus? WriteOffRequestStatus { get; set; }

    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public int? ProcessedByEmployeeId { get; set; }
    public Employee? ProcessedByEmployee { get; set; }

    public string? DecisionNote { get; set; }
    public DateTime? ExecutedAt { get; set; }
}
