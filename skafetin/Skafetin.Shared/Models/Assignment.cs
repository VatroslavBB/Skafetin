namespace Skafetin.Shared.Models;

public class Assignment
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public int AssignmentStatusId { get; set; }
    public AssignmentStatus? AssignmentStatus { get; set; }

    public int? PreviousAssignmentId { get; set; }
    public Assignment? PreviousAssignment { get; set; }

    public int? AssignedByEmployeeId { get; set; }
    public Employee? AssignedByEmployee { get; set; }

    public string? Note { get; set; }
    public string? ReturnNote { get; set; }
    public string? CancelReason { get; set; }

    public DateTime CreatedAt { get; set; }
}
