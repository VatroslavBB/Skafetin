namespace Skafetin.Shared.Models;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
