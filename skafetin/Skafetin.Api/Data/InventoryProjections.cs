using System.Linq.Expressions;
using Skafetin.Shared.DTOs;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Data;

// Definicija odstupanja stoji na jednom mjestu jer je citaju i InventoriesController
// i AiController; dvije kopije bi s vremenom dale dvije razlicite brojke.
public static class InventoryProjections
{
    public static readonly Expression<Func<Inventory, InventorySummaryDto>> ToSummaryDto = i => new InventorySummaryDto
    {
        Total = i.InventoryItems.Count,
        Counted = i.InventoryItems.Count(x => x.IsFound != null),
        Missing = i.InventoryItems.Count(x => x.IsFound == false),
        Damaged = i.InventoryItems.Count(x => x.IsFound == true && x.IsDamaged),
        WrongLocation = i.InventoryItems.Count(x => x.IsFound == true
                                                 && x.FoundLocationId != null
                                                 && x.FoundLocationId != x.ExpectedLocationId)
    };
}
