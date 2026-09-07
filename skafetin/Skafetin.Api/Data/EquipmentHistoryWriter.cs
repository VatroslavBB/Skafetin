using System.Security.Claims;
using Skafetin.Api.Security;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Data;

public static class EquipmentHistoryWriter
{
    public static void Record(
        SkafetinDbContext context,
        Equipment equipment,
        int fromStatusId,
        int fromLocationId,
        ClaimsPrincipal user,
        string? reason = null)
    {
        var statusChanged = fromStatusId != equipment.EquipmentStatusId;
        var locationChanged = fromLocationId != equipment.LocationId;

        if (!statusChanged && !locationChanged)
            return;

        context.EquipmentStatusHistories.Add(new EquipmentStatusHistory
        {
            EquipmentId = equipment.Id,
            FromStatusId = fromStatusId,
            ToStatusId = equipment.EquipmentStatusId,
            FromLocationId = fromLocationId,
            ToLocationId = equipment.LocationId,
            ChangedAt = DateTime.Now,
            ChangedByEmployeeId = ResolveEmployeeId(user),
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
        });
    }

    private static int? ResolveEmployeeId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(AppClaimTypes.EmployeeId)?.Value;

        return int.TryParse(claim, out var employeeId) ? employeeId : null;
    }
}
