namespace Skafetin.Shared.DTOs;

public class UserAdminDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }

    public List<int> RoleIds { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}
