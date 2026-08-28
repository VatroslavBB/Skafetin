namespace Skafetin.Shared.Models;

public class RequestStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();
}
