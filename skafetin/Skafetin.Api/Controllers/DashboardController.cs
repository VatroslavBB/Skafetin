using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private const int EquipmentStatusAssigned = 2;
    private const int EquipmentStatusInService = 3;
    private const int EquipmentStatusMissing = 4;
    private const int EquipmentStatusWrittenOff = 5;

    private const int InventoryStatusOpen = 2;
    private const int InventoryStatusInProgress = 3;

    private const int RequestStatusReceived = 1;
    private const int RequestStatusInProgress = 2;
    private const int RequestStatusApproved = 3;

    private const int RecentActivityCount = 5;

    private readonly SkafetinDbContext _context;

    public DashboardController(SkafetinDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = new DashboardSummaryDto();

        if (User.IsInRole("Admin") || User.IsInRole("InventoryManager"))
            summary.Global = await BuildOverviewAsync(null);

        var locationId = GetClaimId(AppClaimTypes.LocationId);
        if (User.IsInRole("LocationResponsible") && locationId.HasValue)
            summary.Location = await BuildOverviewAsync(locationId.Value);

        var employeeId = GetClaimId(AppClaimTypes.EmployeeId);
        if (employeeId.HasValue)
            summary.Personal = await BuildPersonalAsync(employeeId.Value);

        return Ok(summary);
    }

    private async Task<DashboardOverviewDto> BuildOverviewAsync(int? locationId)
    {
        var equipment = _context.Equipment.AsQueryable();
        var inventories = _context.Inventories.AsQueryable();
        var requests = _context.EquipmentRequests.AsQueryable();

        if (locationId.HasValue)
        {
            equipment = equipment.Where(e => e.LocationId == locationId.Value);
            inventories = inventories.Where(i => i.LocationId == locationId.Value);
            requests = requests.Where(r => r.RequestedByEmployee!.LocationId == locationId.Value);
        }

        var overview = new DashboardOverviewDto
        {
            LocationId = locationId,
            LocationName = locationId.HasValue
                ? await _context.Locations
                    .Where(l => l.Id == locationId.Value)
                    .Select(l => l.Name)
                    .FirstOrDefaultAsync()
                : null,

            TotalEquipment = await equipment.CountAsync(),
            AssignedEquipment = await equipment.CountAsync(e => e.EquipmentStatusId == EquipmentStatusAssigned),
            InServiceEquipment = await equipment.CountAsync(e => e.EquipmentStatusId == EquipmentStatusInService),
            MissingEquipment = await equipment.CountAsync(e => e.EquipmentStatusId == EquipmentStatusMissing),
            WrittenOffEquipment = await equipment.CountAsync(e => e.EquipmentStatusId == EquipmentStatusWrittenOff),

            OpenRequests = await requests.CountAsync(
                r => r.RequestStatusId == RequestStatusReceived
                  || r.RequestStatusId == RequestStatusInProgress
                  || r.RequestStatusId == RequestStatusApproved),

            InventoriesInProgress = await inventories.CountAsync(
                i => i.InventoryStatusId == InventoryStatusOpen
                  || i.InventoryStatusId == InventoryStatusInProgress),

            TotalPurchaseValue = await equipment.SumAsync(e => (decimal?)e.PurchaseValue) ?? 0m
        };

        overview.RecentActivity = await BuildRecentActivityAsync(locationId, null);

        return overview;
    }

    private async Task<PersonalOverviewDto> BuildPersonalAsync(int employeeId)
    {
        return new PersonalOverviewDto
        {
            ActiveAssignments = await _context.Assignments
                .CountAsync(a => a.EmployeeId == employeeId && a.ReturnedAt == null),

            OpenRequests = await _context.EquipmentRequests
                .CountAsync(r => r.RequestedByEmployeeId == employeeId
                              && (r.RequestStatusId == RequestStatusReceived
                               || r.RequestStatusId == RequestStatusInProgress
                               || r.RequestStatusId == RequestStatusApproved)),

            RecentActivity = await BuildRecentActivityAsync(null, employeeId)
        };
    }

    private async Task<List<RecentActivityDto>> BuildRecentActivityAsync(int? locationId, int? employeeId)
    {
        var assignments = _context.Assignments.AsQueryable();
        var writeOffs = _context.WriteOffRequests.AsQueryable();
        var requests = _context.EquipmentRequests.AsQueryable();

        if (locationId.HasValue)
        {
            assignments = assignments.Where(a => a.Equipment!.LocationId == locationId.Value);
            writeOffs = writeOffs.Where(w => w.Equipment!.LocationId == locationId.Value);
            requests = requests.Where(r => r.RequestedByEmployee!.LocationId == locationId.Value);
        }

        if (employeeId.HasValue)
        {
            assignments = assignments.Where(a => a.EmployeeId == employeeId.Value);
            writeOffs = writeOffs.Where(w => w.RequestedByEmployeeId == employeeId.Value);
            requests = requests.Where(r => r.RequestedByEmployeeId == employeeId.Value);
        }

        var recentAssignments = await assignments
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .Take(RecentActivityCount)
            .Select(a => new RecentActivityDto
            {
                OccurredAt = a.CreatedAt,
                Kind = "Zaduženje",
                Title = a.Equipment!.InventoryNumber + " - " + a.Equipment!.Name,
                Description = a.Employee!.FirstName + " " + a.Employee!.LastName,
                EquipmentId = a.EquipmentId
            })
            .ToListAsync();

        var recentWriteOffs = await writeOffs
            .OrderByDescending(w => w.CreatedAt)
            .ThenByDescending(w => w.Id)
            .Take(RecentActivityCount)
            .Select(w => new RecentActivityDto
            {
                OccurredAt = w.CreatedAt,
                Kind = "Otpis",
                Title = w.Equipment!.InventoryNumber + " - " + w.Equipment!.Name,
                Description = w.WriteOffRequestStatus!.Name,
                EquipmentId = w.EquipmentId
            })
            .ToListAsync();

        var recentRequests = await requests
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Take(RecentActivityCount)
            .Select(r => new RecentActivityDto
            {
                OccurredAt = r.CreatedAt,
                Kind = "Zahtjev",
                Title = r.Title,
                Description = r.RequestStatus!.Name,
                EquipmentId = null
            })
            .ToListAsync();

        return recentAssignments
            .Concat(recentWriteOffs)
            .Concat(recentRequests)
            .OrderByDescending(x => x.OccurredAt)
            .Take(RecentActivityCount)
            .ToList();
    }

    private int? GetClaimId(string claimType)
    {
        var claim = User.FindFirst(claimType)?.Value;

        return int.TryParse(claim, out var id) ? id : null;
    }
}
