using Microsoft.EntityFrameworkCore;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Data;

/// <summary>
/// Demo slike i dokumenti opreme. Same datoteke stoje u SeedFiles/equipment i
/// commitaju se u git; seed ih kopira u mapu za upload (koja je u .gitignore-u)
/// i tek onda upisuje metapodatke, da nakon kloniranja repozitorija profil
/// opreme nema zapise koji pokazuju na nepostojeće datoteke.
/// Slike su vlastite ilustracije - generira ih docs/alati/generiraj-seed-medije.py.
/// </summary>
public static partial class SeedData
{
    private const string KindImage = "Image";
    private const string KindDocument = "Document";

    private sealed record SeedMedia(
        string InventoryNumber,
        string FileName,
        string Title,
        string Kind,
        DateTime UploadedAt);

    private static readonly SeedMedia[] MediaFiles =
    [
        new("INV-0001", "inv-0001-dell-latitude-5540.jpg", "Prijenosno računalo Dell Latitude 5540", KindImage, new DateTime(2026, 2, 5)),
        new("INV-0002", "inv-0002-lenovo-thinkpad-t14.jpg", "Prijenosno računalo Lenovo ThinkPad T14", KindImage, new DateTime(2026, 2, 6)),
        new("INV-0004", "inv-0004-hp-elitedesk-800.jpg", "Stolno računalo HP EliteDesk 800", KindImage, new DateTime(2026, 2, 7)),
        new("INV-0005", "inv-0005-lenovo-thinkcentre-m70q.jpg", "Stolno računalo Lenovo ThinkCentre M70q", KindImage, new DateTime(2026, 2, 8)),
        new("INV-0006", "inv-0006-dell-p2422h.jpg", "Monitor Dell P2422H 24\"", KindImage, new DateTime(2026, 2, 9)),
        new("INV-0009", "inv-0009-hp-laserjet-m404dn.jpg", "Pisač HP LaserJet Pro M404dn", KindImage, new DateTime(2026, 2, 10)),
        new("INV-0010", "inv-0010-canon-mf443dw.jpg", "Multifunkcijski uređaj Canon i-SENSYS MF443dw", KindImage, new DateTime(2026, 2, 11)),
        new("INV-0011", "inv-0011-epson-ds-770.jpg", "Skener Epson WorkForce DS-770", KindImage, new DateTime(2026, 2, 12)),
        new("INV-0012", "inv-0012-epson-eb-w52.jpg", "Projektor Epson EB-W52", KindImage, new DateTime(2026, 2, 13)),
        new("INV-0013", "inv-0013-galaxy-tab-a9.jpg", "Tablet Samsung Galaxy Tab A9+", KindImage, new DateTime(2026, 2, 14)),
        new("INV-0014", "inv-0014-fujitsu-esprimo-p556.jpg", "Stolno računalo Fujitsu Esprimo P556", KindImage, new DateTime(2026, 2, 15)),
        new("INV-0101", "inv-0101-cisco-catalyst-1000.jpg", "Preklopnik Cisco Catalyst 1000 24-port", KindImage, new DateTime(2026, 2, 16)),
        new("INV-0103", "inv-0103-mikrotik-hex-s.jpg", "Usmjerivač MikroTik hEX S", KindImage, new DateTime(2026, 2, 17)),
        new("INV-0104", "inv-0104-unifi-u6-lite.jpg", "Pristupna točka Ubiquiti UniFi U6 Lite", KindImage, new DateTime(2026, 2, 18)),
        new("INV-0106", "inv-0106-apc-smart-ups-1500.jpg", "Neprekidno napajanje APC Smart-UPS 1500VA", KindImage, new DateTime(2026, 2, 19)),
        new("INV-0201", "inv-0201-uredski-stol.jpg", "Uredski stol 160x80", KindImage, new DateTime(2026, 2, 20)),
        new("INV-0203", "inv-0203-stolica-ergo-plus.jpg", "Uredska stolica Ergo Plus", KindImage, new DateTime(2026, 2, 21)),
        new("INV-0205", "inv-0205-ormar-za-spise.jpg", "Ormar za spise 180x80", KindImage, new DateTime(2026, 2, 22)),
        new("INV-0206", "inv-0206-konferencijski-stol.jpg", "Konferencijski stol 240x100", KindImage, new DateTime(2026, 2, 23)),
        new("INV-0207", "inv-0207-bolnicki-krevet.jpg", "Bolnički krevet na kotačima", KindImage, new DateTime(2026, 2, 24)),
        new("INV-0208", "inv-0208-metalni-regal.jpg", "Metalni regal 200x100", KindImage, new DateTime(2026, 2, 25)),
        new("INV-0301", "inv-0301-bosch-gsb-18v-55.jpg", "Udarna bušilica Bosch GSB 18V-55", KindImage, new DateTime(2026, 2, 26)),
        new("INV-0302", "inv-0302-stanley-set-alata.jpg", "Set ručnog alata 108 dijelova", KindImage, new DateTime(2026, 2, 27)),
        new("INV-0303", "inv-0303-aluminijske-ljestve.jpg", "Aluminijske ljestve 8 stepenica", KindImage, new DateTime(2026, 2, 28)),
        new("INV-0304", "inv-0304-makita-ga5030.jpg", "Kutna brusilica Makita GA5030", KindImage, new DateTime(2026, 3, 1)),
        new("INV-0305", "inv-0305-husqvarna-lc-140.jpg", "Kosilica Husqvarna LC 140", KindImage, new DateTime(2026, 3, 2)),
        new("INV-0401", "inv-0401-daikin-sensira.jpg", "Klima uređaj Daikin Sensira 3,5 kW", KindImage, new DateTime(2026, 3, 3)),
        new("INV-0403", "inv-0403-vivax-acp-12.jpg", "Klima uređaj Vivax ACP-12 3,5 kW", KindImage, new DateTime(2026, 3, 4)),
        new("INV-0404", "inv-0404-boneco-h300.jpg", "Prijenosni ovlaživač zraka Boneco H300", KindImage, new DateTime(2026, 3, 5)),
        new("INV-0501", "inv-0501-omron-m7.jpg", "Tlakomjer Omron M7 Intelli IT", KindImage, new DateTime(2026, 3, 6)),
        new("INV-0502", "inv-0502-schiller-cardiovit-at-1.jpg", "EKG uređaj Schiller Cardiovit AT-1", KindImage, new DateTime(2026, 3, 7)),
        new("INV-0503", "inv-0503-zoll-aed-plus.jpg", "Defibrilator Zoll AED Plus", KindImage, new DateTime(2026, 3, 8)),
        new("INV-0504", "inv-0504-meyra-eurochair.jpg", "Invalidska kolica Meyra Eurochair", KindImage, new DateTime(2026, 3, 9)),
        new("INV-0505", "inv-0505-omron-c102.jpg", "Inhalator Omron C102 Total", KindImage, new DateTime(2026, 3, 10)),
        new("INV-0601", "inv-0601-gorenje-rb615few5.jpg", "Hladnjak Gorenje RB615FEW5", KindImage, new DateTime(2026, 3, 11)),
        new("INV-0602", "inv-0602-jura-e8.jpg", "Aparat za kavu Jura E8", KindImage, new DateTime(2026, 3, 12)),
        new("INV-0603", "inv-0603-fellowes-79ci.jpg", "Uništavač dokumenata Fellowes 79Ci", KindImage, new DateTime(2026, 3, 13)),
        new("INV-0604", "inv-0604-skoda-octavia.jpg", "Službeno vozilo Škoda Octavia", KindImage, new DateTime(2026, 3, 14)),

        new("INV-0001", "inv-0001-jamstveni-list.pdf", "Jamstveni list", KindDocument, new DateTime(2023, 3, 20)),
        new("INV-0106", "inv-0106-servisni-nalog.pdf", "Servisni nalog", KindDocument, new DateTime(2026, 5, 12)),
        new("INV-0013", "inv-0013-zapisnik-o-manjku.pdf", "Zapisnik o utvrđenom manjku", KindDocument, new DateTime(2026, 1, 27)),
        new("INV-0014", "inv-0014-zapisnik-o-otpisu.pdf", "Zapisnik o otpisu", KindDocument, new DateTime(2026, 2, 18)),
        new("INV-0505", "inv-0505-zapisnik-o-otpisu.pdf", "Zapisnik o otpisu", KindDocument, new DateTime(2026, 3, 20))
    ];

    private static async Task SeedEquipmentMediaAsync(
        SkafetinDbContext db,
        string? seedFilesDirectory,
        string? uploadDirectory,
        ILogger? logger = null)
    {
        if (await db.EquipmentMedia.AnyAsync())
            return;

        if (string.IsNullOrWhiteSpace(seedFilesDirectory) || string.IsNullOrWhiteSpace(uploadDirectory))
        {
            logger?.LogWarning("Mediji opreme nisu zasijani jer putanje do datoteka nisu proslijeđene.");
            return;
        }

        if (!Directory.Exists(seedFilesDirectory))
        {
            logger?.LogWarning(
                "Mapa s demo datotekama {Directory} ne postoji, mediji opreme se preskaču.",
                seedFilesDirectory);
            return;
        }

        Directory.CreateDirectory(uploadDirectory);

        var equipment = await db.Equipment.ToDictionaryAsync(e => e.InventoryNumber, e => e.Id);
        var uploadedBy = await db.Employees
            .Where(e => e.Email == "marko.juric@skafetin.hr")
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();

        // Naslovna je prva slika svakog komada opreme; dokumenti nikad nisu naslovni
        // (isto pravilo vrijedi i u EquipmentMediaController.SetCover).
        var coverTaken = new HashSet<int>();
        var media = new List<EquipmentMedia>();

        foreach (var item in MediaFiles)
        {
            if (!equipment.TryGetValue(item.InventoryNumber, out var equipmentId))
            {
                logger?.LogWarning(
                    "Datoteka {FileName} preskočena jer oprema {InventoryNumber} ne postoji.",
                    item.FileName,
                    item.InventoryNumber);
                continue;
            }

            var source = Path.Combine(seedFilesDirectory, item.FileName);
            if (!File.Exists(source))
            {
                logger?.LogWarning("Demo datoteka {Source} ne postoji, preskačem.", source);
                continue;
            }

            var target = Path.Combine(uploadDirectory, item.FileName);
            File.Copy(source, target, overwrite: true);

            var isCover = item.Kind == KindImage && coverTaken.Add(equipmentId);

            media.Add(new EquipmentMedia
            {
                EquipmentId = equipmentId,
                Title = item.Title,
                MediaKind = item.Kind,
                IsCover = isCover,
                OriginalFileName = item.FileName,
                StoredFileName = item.FileName,
                ContentType = item.Kind == KindImage ? "image/jpeg" : "application/pdf",
                FileSize = new FileInfo(target).Length,
                UploadedAt = item.UploadedAt,
                UploadedByEmployeeId = uploadedBy
            });
        }

        db.EquipmentMedia.AddRange(media);
        await db.SaveChangesAsync();

        logger?.LogInformation("Zasijano {Count} datoteka opreme.", media.Count);
    }
}
