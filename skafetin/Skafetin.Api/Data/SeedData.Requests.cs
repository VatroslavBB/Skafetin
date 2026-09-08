using Microsoft.EntityFrameworkCore;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Data;

/// <summary>
/// Demo zahtjevi za opremom i zahtjevi za otpisom. Oba skupa pokrivaju sve
/// vrijednosti svog šifrarnika, da se filtri i prikaz statusa vide na demu.
/// Prijelazi statusa poštuju IsAllowedTransition iz pripadnih kontrolera.
/// </summary>
public static partial class SeedData
{
    // Statusi zahtjeva (HasData): 1 Zaprimljeno, 2 U obradi, 3 Odobreno,
    //                             4 Odbijeno, 5 Realizirano, 6 Zatvoreno.
    private const int RequestReceived = 1;
    private const int RequestInProgress = 2;
    private const int RequestApproved = 3;
    private const int RequestRejected = 4;
    private const int RequestFulfilled = 5;
    private const int RequestClosed = 6;

    // Statusi otpisa (HasData): 1 Zaprimljeno, 2 U obradi, 3 Odobreno,
    //                           4 Odbijeno, 5 Provedeno.
    private const int WriteOffReceived = 1;
    private const int WriteOffInProgress = 2;
    private const int WriteOffApproved = 3;
    private const int WriteOffRejected = 4;
    private const int WriteOffExecuted = 5;

    private static async Task SeedEquipmentRequestsAsync(SkafetinDbContext db)
    {
        if (await db.EquipmentRequests.AnyAsync())
            return;

        var employees = await db.Employees.ToDictionaryAsync(e => e.Email, e => e.Id);
        var equipment = await db.Equipment.ToDictionaryAsync(e => e.InventoryNumber, e => e.Id);

        // Kategorije (HasData): 1 Računalo, 2 Mrežna oprema, 3 Namještaj,
        //                       4 Alat, 5 Klimatizacija, 6 Medicinska oprema, 7 Ostalo.
        db.EquipmentRequests.AddRange(
            // Zaprimljeno - još nitko nije preuzeo zahtjev u obradu.
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["josip.matic@skafetin.hr"],
                EquipmentCategoryId = 1,
                Title = "Prijenosno računalo za informatičku učionicu",
                Description = "Postojeće stolno računalo u učionici ne pokreće alate koji se koriste "
                            + "na nastavi. Molim nabavu prijenosnog računala srednje klase.",
                RequestStatusId = RequestReceived,
                CreatedAt = new DateTime(2026, 6, 15)
            },
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["nikolina.radic@skafetin.hr"],
                EquipmentCategoryId = 6,
                Title = "Drugi tlakomjer za ambulantu",
                Description = "U ambulanti radi jedan tlakomjer, pa se pri većem broju pacijenata "
                            + "stvara zastoj. Molim nabavu još jednog uređaja.",
                RequestStatusId = RequestReceived,
                CreatedAt = new DateTime(2026, 7, 2)
            },

            // U obradi - preuzeto, odluka još nije donesena, pa ProcessedAt ostaje prazan.
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["maja.saric@skafetin.hr"],
                EquipmentCategoryId = 1,
                Title = "Zamjenski monitor za tajništvo",
                Description = "Monitor u tajništvu povremeno gubi sliku. Molim zamjenu.",
                ReplacementForEquipmentId = equipment["INV-0008"],
                RequestStatusId = RequestInProgress,
                CreatedAt = new DateTime(2026, 5, 28)
            },

            // Odobreno - odluka donesena, oprema još nije dodijeljena.
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["damir.lovric@skafetin.hr"],
                EquipmentCategoryId = 4,
                Title = "Akumulatorska bušilica za domara",
                Description = "Postojeća bušilica dijeli se s domom zdravlja, pa često nije dostupna "
                            + "kad su potrebni hitni popravci.",
                RequestStatusId = RequestApproved,
                CreatedAt = new DateTime(2026, 4, 9),
                ProcessedAt = new DateTime(2026, 4, 21),
                ProcessedByEmployeeId = employees["marko.juric@skafetin.hr"],
                DecisionNote = "Odobreno, nabava se planira u trećem kvartalu."
            },

            // Odbijeno - obrazloženje je obavezno (vidi EquipmentRequestsController).
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["sanja.grubisic@skafetin.hr"],
                EquipmentCategoryId = 7,
                Title = "Aparat za kavu za dnevni boravak",
                Description = "Korisnici i djelatnici koriste zajednički prostor, pa bi aparat za kavu "
                            + "bio koristan.",
                RequestStatusId = RequestRejected,
                CreatedAt = new DateTime(2026, 3, 11),
                ProcessedAt = new DateTime(2026, 3, 19),
                ProcessedByEmployeeId = employees["petra.kovacevic@skafetin.hr"],
                DecisionNote = "Odbijeno: nabava nije predviđena planom za tekuću godinu."
            },

            // Realizirano - zahtjevu je dodijeljena konkretna oprema.
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["josip.matic@skafetin.hr"],
                EquipmentCategoryId = 2,
                Title = "Pristupna točka za drugi kat škole",
                Description = "Na drugom katu nema bežične mreže u učionicama uz istočno stubište.",
                RequestStatusId = RequestFulfilled,
                CreatedAt = new DateTime(2026, 1, 20),
                ProcessedAt = new DateTime(2026, 2, 3),
                ProcessedByEmployeeId = employees["marko.juric@skafetin.hr"],
                DecisionNote = "Odobreno, dodjeljuje se uređaj sa skladišta.",
                ResultingEquipmentId = equipment["INV-0105"]
            },

            // Zatvoreno nakon realizacije.
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["luka.bilic@skafetin.hr"],
                EquipmentCategoryId = 6,
                Title = "Zamjenski inhalator",
                Description = "Postojećem inhalatoru istekao je vijek trajanja i predan je na otpis.",
                ReplacementForEquipmentId = equipment["INV-0505"],
                RequestStatusId = RequestClosed,
                CreatedAt = new DateTime(2026, 2, 24),
                ProcessedAt = new DateTime(2026, 3, 6),
                ProcessedByEmployeeId = employees["marko.juric@skafetin.hr"],
                DecisionNote = "Odobreno, zamjenski uređaj preuzet iz zalihe.",
                ResultingEquipmentId = equipment["INV-0504"]
            },

            // Zatvoreno nakon odbijanja.
            new EquipmentRequest
            {
                RequestedByEmployeeId = employees["nikolina.radic@skafetin.hr"],
                EquipmentCategoryId = 5,
                Title = "Klima uređaj za čekaonicu",
                Description = "Ljeti je u čekaonici izrazito vruće.",
                RequestStatusId = RequestClosed,
                CreatedAt = new DateTime(2025, 8, 12),
                ProcessedAt = new DateTime(2025, 8, 26),
                ProcessedByEmployeeId = employees["petra.kovacevic@skafetin.hr"],
                DecisionNote = "Odbijeno: prostor je u planu obnove, ugradnja se odgađa do radova."
            }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedWriteOffRequestsAsync(SkafetinDbContext db)
    {
        if (await db.WriteOffRequests.AnyAsync())
            return;

        var employees = await db.Employees.ToDictionaryAsync(e => e.Email, e => e.Id);
        var equipment = await db.Equipment.ToDictionaryAsync(e => e.InventoryNumber, e => e.Id);

        // Za jedan komad opreme smije postojati najviše jedan otvoren zahtjev
        // (WriteOffRequestsController), pa svaki komad ovdje ima najviše jedan.
        db.WriteOffRequests.AddRange(
            // Provedeno - ova tri komada imaju status Otpisano u SeedEquipmentAsync.
            // Datum provedbe mora se poklapati s poviješću statusa (SeedData.History.cs).
            new WriteOffRequest
            {
                EquipmentId = equipment["INV-0014"],
                RequestedByEmployeeId = employees["tomislav.vukovic@skafetin.hr"],
                WriteOffRequestStatusId = WriteOffExecuted,
                Reason = "Računalo je dotrajalo, popravak nije isplativ.",
                CreatedAt = new DateTime(2026, 1, 28),
                ProcessedAt = new DateTime(2026, 2, 9),
                ProcessedByEmployeeId = employees["marko.juric@skafetin.hr"],
                DecisionNote = "Odobreno, oprema se isključuje iz uporabe.",
                ExecutedAt = new DateTime(2026, 2, 18)
            },
            new WriteOffRequest
            {
                EquipmentId = equipment["INV-0208"],
                RequestedByEmployeeId = employees["tomislav.vukovic@skafetin.hr"],
                WriteOffRequestStatusId = WriteOffExecuted,
                Reason = "Korozija nosača, regal više nije siguran za uporabu.",
                CreatedAt = new DateTime(2025, 5, 14),
                ProcessedAt = new DateTime(2025, 5, 26),
                ProcessedByEmployeeId = employees["marko.juric@skafetin.hr"],
                DecisionNote = "Odobreno na temelju očevida u skladištu.",
                ExecutedAt = new DateTime(2025, 6, 5)
            },
            new WriteOffRequest
            {
                EquipmentId = equipment["INV-0505"],
                RequestedByEmployeeId = employees["luka.bilic@skafetin.hr"],
                WriteOffRequestStatusId = WriteOffExecuted,
                Reason = "Istekao vijek trajanja medicinskog uređaja.",
                CreatedAt = new DateTime(2026, 2, 20),
                ProcessedAt = new DateTime(2026, 3, 4),
                ProcessedByEmployeeId = employees["petra.kovacevic@skafetin.hr"],
                DecisionNote = "Odobreno, nabavljen je zamjenski uređaj.",
                ExecutedAt = new DateTime(2026, 3, 20)
            },

            // Odobreno, ali još nije provedeno - oprema zadržava status Nedostaje.
            new WriteOffRequest
            {
                EquipmentId = equipment["INV-0304"],
                RequestedByEmployeeId = employees["damir.lovric@skafetin.hr"],
                WriteOffRequestStatusId = WriteOffApproved,
                Reason = "Brusilica nije pronađena na inventuri 2025. i vjerojatno je nepovratno izgubljena.",
                CreatedAt = new DateTime(2026, 4, 6),
                ProcessedAt = new DateTime(2026, 4, 20),
                ProcessedByEmployeeId = employees["marko.juric@skafetin.hr"],
                DecisionNote = "Odobreno, provedba čeka zapisnik o manjku."
            },

            // Odbijeno - obrazloženje je obavezno.
            new WriteOffRequest
            {
                EquipmentId = equipment["INV-0005"],
                RequestedByEmployeeId = employees["ana.peric@skafetin.hr"],
                WriteOffRequestStatusId = WriteOffRejected,
                Reason = "Računalo se ne pokreće, predlažem otpis.",
                CreatedAt = new DateTime(2026, 4, 15),
                ProcessedAt = new DateTime(2026, 4, 24),
                ProcessedByEmployeeId = employees["marko.juric@skafetin.hr"],
                DecisionNote = "Odbijeno: servis je procijenio da je popravak isplativ."
            },

            // U obradi - odluka još nije donesena.
            new WriteOffRequest
            {
                EquipmentId = equipment["INV-0403"],
                RequestedByEmployeeId = employees["nikolina.radic@skafetin.hr"],
                WriteOffRequestStatusId = WriteOffInProgress,
                Reason = "Klima uređaj opetovano curi, servis je bio bez trajnog učinka.",
                CreatedAt = new DateTime(2026, 6, 22)
            },

            // Zaprimljeno - čeka preuzimanje u obradu.
            new WriteOffRequest
            {
                EquipmentId = equipment["INV-0013"],
                RequestedByEmployeeId = employees["maja.saric@skafetin.hr"],
                WriteOffRequestStatusId = WriteOffReceived,
                Reason = "Tablet nije pronađen na inventuri, predlažem otpis zbog manjka.",
                CreatedAt = new DateTime(2026, 7, 8)
            }
        );

        await db.SaveChangesAsync();
    }
}
