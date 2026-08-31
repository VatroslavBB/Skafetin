namespace Skafetin.Shared.DTOs;

public class AssignmentDto
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int AssignmentStatusId { get; set; }
    public string AssignmentStatusName { get; set; } = string.Empty;
    public int? PreviousAssignmentId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

