using Microsoft.EntityFrameworkCore;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Data;

/// <summary>
/// Povijest statusa opreme. Ne piše se ručno nego se izvodi iz već zasijanih
/// zaduženja, pa je zajamčeno da se poklapa s njima: svako zaduženje daje prijelaz
/// u Zaduženo, svaki povrat prijelaz natrag u Na skladištu. Završna stanja
/// (servis, manjak, otpis) dopisuju se iz tablice ClosingChanges.
/// </summary>
public static partial class SeedData
{
    // Statusi opreme (HasData): 1 Na skladištu, 2 Zaduženo, 3 Na servisu,
    //                           4 Nedostaje, 5 Otpisano.
    private const int EquipmentInStock = 1;
    private const int EquipmentAssigned = 2;
    private const int EquipmentInService = 3;
    private const int EquipmentMissing = 4;
    private const int EquipmentWrittenOff = 5;

    // Statusi zaduženja: 1 Aktivno, 2 Vraćeno, 3 Premješteno, 4 Stornirano.
    private const int AssignmentReturned = 2;
    private const int AssignmentCanceled = 4;

    /// <summary>
    /// Prijelaz kojim oprema završava u današnjem stanju. Datum mora biti nakon
    /// posljednjeg zaduženja, a kod otpisa se mora poklapati s ExecutedAt
    /// odgovarajućeg zahtjeva u SeedData.Requests.cs.
    /// </summary>
    private sealed record ClosingChange(string InventoryNumber, int ToStatusId, DateTime ChangedAt, string Reason);

    private static readonly ClosingChange[] ClosingChanges =
    [
        new("INV-0005", EquipmentInService, new DateTime(2026, 4, 14), "Poslano u servis, uređaj se ne pokreće."),
        new("INV-0106", EquipmentInService, new DateTime(2026, 5, 12), "Poslano u servis radi zamjene baterijskog modula."),
        new("INV-0305", EquipmentInService, new DateTime(2026, 3, 3), "Poslano u servis motora."),
        new("INV-0403", EquipmentInService, new DateTime(2026, 6, 9), "Poslano u servis, curi kondenzat."),

        new("INV-0013", EquipmentMissing, new DateTime(2026, 1, 26), "Nije pronađeno na godišnjoj inventuri."),
        new("INV-0304", EquipmentMissing, new DateTime(2025, 11, 10), "Nije pronađena na inventuri 2025."),

        new("INV-0014", EquipmentWrittenOff, new DateTime(2026, 2, 18), "Proveden otpis: računalo je dotrajalo, popravak nije isplativ."),
        new("INV-0208", EquipmentWrittenOff, new DateTime(2025, 6, 5), "Proveden otpis: korozija nosača, regal više nije siguran za uporabu."),
        new("INV-0505", EquipmentWrittenOff, new DateTime(2026, 3, 20), "Proveden otpis: istekao vijek trajanja medicinskog uređaja.")
    ];

    private static async Task SeedEquipmentStatusHistoryAsync(SkafetinDbContext db, ILogger? logger = null)
    {
        if (await db.EquipmentStatusHistories.AnyAsync())
            return;

        var equipment = await db.Equipment.ToListAsync();
        var assignments = await db.Assignments
            .Include(a => a.Employee)
            .ToListAsync();
        var closingByInventoryNumber = ClosingChanges.ToDictionary(c => c.InventoryNumber);

        // Promjene evidentira voditelj imovine; lokacija se zaduženjem ne mijenja (P2),
        // pa su FromLocationId i ToLocationId uvijek isti.
        var changedBy = await db.Employees
            .Where(e => e.Email == "marko.juric@skafetin.hr")
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();

        var history = new List<EquipmentStatusHistory>();

        foreach (var item in equipment)
        {
            var currentStatusId = EquipmentInStock;

            EquipmentStatusHistory Record(int? fromStatusId, int toStatusId, DateTime changedAt, string reason) =>
                new()
                {
                    EquipmentId = item.Id,
                    FromStatusId = fromStatusId,
                    ToStatusId = toStatusId,
                    FromLocationId = item.LocationId,
                    ToLocationId = item.LocationId,
                    ChangedAt = changedAt,
                    ChangedByEmployeeId = changedBy,
                    Reason = reason
                };

            // Unos u evidenciju - jedini zapis bez prethodnog statusa.
            history.Add(Record(null, EquipmentInStock, item.CreatedAt, "Unos u evidenciju imovine."));

            var timeline = assignments
                .Where(a => a.EquipmentId == item.Id
                         && a.AssignmentStatusId != AssignmentCanceled)
                .OrderBy(a => a.AssignedAt)
                .ThenBy(a => a.Id);

            foreach (var assignment in timeline)
            {
                var fullName = assignment.Employee is null
                    ? "zaposlenika"
                    : $"{assignment.Employee.FirstName} {assignment.Employee.LastName}";

                if (currentStatusId != EquipmentAssigned)
                {
                    history.Add(Record(currentStatusId, EquipmentAssigned, assignment.AssignedAt,
                        $"Zaduženo na {fullName}."));
                    currentStatusId = EquipmentAssigned;
                }

                // Prijenos ne vraća opremu u skladište nego ju preuzima sljedeći
                // zaposlenik, pa se za Premješteno ne bilježi povratak (pravilo 4).
                if (assignment.AssignmentStatusId == AssignmentReturned && assignment.ReturnedAt.HasValue)
                {
                    history.Add(Record(currentStatusId, EquipmentInStock, assignment.ReturnedAt.Value,
                        "Povrat opreme u skladište."));
                    currentStatusId = EquipmentInStock;
                }
            }

            if (closingByInventoryNumber.TryGetValue(item.InventoryNumber, out var closing))
            {
                history.Add(Record(currentStatusId, closing.ToStatusId, closing.ChangedAt, closing.Reason));
                currentStatusId = closing.ToStatusId;
            }

            // Zaštita za buduće izmjene seeda: ako se popis opreme ili zaduženja
            // promijeni, a ClosingChanges ne, povijest bi tiho završila u krivom
            // statusu. Upozorenje u logu pokazuje na koji komad treba pogledati.
            if (currentStatusId != item.EquipmentStatusId)
            {
                logger?.LogWarning(
                    "Povijest opreme {InventoryNumber} završava u statusu {HistoryStatusId}, "
                    + "a oprema ima status {EquipmentStatusId}. Uskladi ClosingChanges u SeedData.History.cs.",
                    item.InventoryNumber,
                    currentStatusId,
                    item.EquipmentStatusId);
            }
        }

        db.EquipmentStatusHistories.AddRange(history);
        await db.SaveChangesAsync();
    }
}
