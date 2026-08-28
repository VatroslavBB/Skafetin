namespace Skafetin.Shared.Models;

public class AssignmentStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
