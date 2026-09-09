namespace Skafetin.Shared.DTOs;

public class DashboardSummaryDto
{
    public DashboardOverviewDto? Global { get; set; }
    public DashboardOverviewDto? Location { get; set; }
    public PersonalOverviewDto? Personal { get; set; }
}

public class DashboardOverviewDto
{
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }

    public int TotalEquipment { get; set; }
    public int AssignedEquipment { get; set; }
    public int InServiceEquipment { get; set; }
    public int MissingEquipment { get; set; }
    public int WrittenOffEquipment { get; set; }

    public int OpenRequests { get; set; }
    public int InventoriesInProgress { get; set; }

    public decimal TotalPurchaseValue { get; set; }

    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class PersonalOverviewDto
{
    public int ActiveAssignments { get; set; }
    public int OpenRequests { get; set; }

    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}
