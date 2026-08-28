namespace Skafetin.Shared.Models;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Zip { get; set; }
    public string City { get; set; } = string.Empty;

    public int LocationTypeId { get; set; }
    public LocationType? LocationType { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
