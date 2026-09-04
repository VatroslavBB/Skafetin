using Skafetin.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Skafetin.Api.Data;
public static class SeedData
{
    public static async Task SeedAsync(SkafetinDbContext db, ILogger? logger = null)
    {
        await SeedLocationsAsync(db);
        await SeedEmployeesAsync(db);
        await SeedEquipmentAsync(db);
        await SeedAssignmentsAsync(db);
        await SeedInventoriesAsync(db);
        await AppUserSeeder.SeedAsync(db, logger);
        await db.SaveChangesAsync();
    }

    private static async Task SeedLocationsAsync(SkafetinDbContext db)
    {
        if (await db.Locations.AnyAsync())
            return;

        db.Locations.AddRange(
            new Location { Name = "Županijska uprava", Address = "Domovinskog rata 2", Zip = 21000, City = "Split", LocationTypeId = 1, IsActive = true },
            new Location { Name = "Upravni odjel za imovinu", Address = "Vukovarska 1", Zip = 21000, City = "Split", LocationTypeId = 1, IsActive = true },
            new Location { Name = "OŠ Kamen-Šine", Address = "Put Sv. Lovre 3", Zip = 21000, City = "Split", LocationTypeId = 2, IsActive = true },
            new Location { Name = "SŠ Braće Radić", Address = "Trg kralja Tomislava 8", Zip = 21230, City = "Sinj", LocationTypeId = 2, IsActive = true },
            new Location { Name = "Dom zdravlja Solin", Address = "Kralja Zvonimira 24", Zip = 21210, City = "Solin", LocationTypeId = 3, IsActive = true },
            new Location { Name = "Ambulanta Omiš", Address = "Fošal 12", Zip = 21310, City = "Omiš", LocationTypeId = 3, IsActive = true },
            new Location { Name = "Dom za starije Trogir", Address = "Put Mulina 5", Zip = 21220, City = "Trogir", LocationTypeId = 4, IsActive = true },
            new Location { Name = "Staro skladište Dugopolje", Address = "Industrijska 4", Zip = 21204, City = "Dugopolje", LocationTypeId = 1, IsActive = false }
        );

        // lokacije se spremaju odmah jer zaposlenici trebaju njihove Id-eve
        await db.SaveChangesAsync();
    }

    private static async Task SeedEmployeesAsync(SkafetinDbContext db)
    {
        if (await db.Employees.AnyAsync())
            return;

        var locations = await db.Locations.ToDictionaryAsync(loc => loc.Name, loc => loc.Id);

        db.Employees.AddRange(
            new Employee { FirstName = "Ivana", LastName = "Barišić", Email = "ivana.barisic@skafetin.hr", Phone = "021 400 101", JobTitle = "Pročelnica upravnog odjela", LocationId = locations["Županijska uprava"], IsActive = true },
            new Employee { FirstName = "Marko", LastName = "Jurić", Email = "marko.juric@skafetin.hr", Phone = "021 400 102", JobTitle = "Voditelj imovine", LocationId = locations["Upravni odjel za imovinu"], IsActive = true },
            new Employee { FirstName = "Petra", LastName = "Kovačević", Email = "petra.kovacevic@skafetin.hr", Phone = "021 400 103", JobTitle = "Referentica za imovinu", LocationId = locations["Upravni odjel za imovinu"], IsActive = true },
            new Employee { FirstName = "Tomislav", LastName = "Vuković", Email = "tomislav.vukovic@skafetin.hr", Phone = "021 400 104", JobTitle = "Skladištar", LocationId = locations["Upravni odjel za imovinu"], IsActive = true },
            new Employee { FirstName = "Ana", LastName = "Perić", Email = "ana.peric@skafetin.hr", Phone = "021 550 201", JobTitle = "Ravnateljica škole", LocationId = locations["OŠ Kamen-Šine"], IsActive = true },
            new Employee { FirstName = "Josip", LastName = "Matić", Email = "josip.matic@skafetin.hr", Phone = "021 550 202", JobTitle = "Učitelj informatike", LocationId = locations["OŠ Kamen-Šine"], IsActive = true },
            new Employee { FirstName = "Maja", LastName = "Šarić", Email = "maja.saric@skafetin.hr", Phone = "021 660 301", JobTitle = "Tajnica škole", LocationId = locations["SŠ Braće Radić"], IsActive = true },
            new Employee { FirstName = "Luka", LastName = "Bilić", Email = "luka.bilic@skafetin.hr", Phone = "021 770 401", JobTitle = "Voditelj tehničke službe", LocationId = locations["Dom zdravlja Solin"], IsActive = true },
            new Employee { FirstName = "Nikolina", LastName = "Radić", Email = "nikolina.radic@skafetin.hr", Phone = "021 770 402", JobTitle = "Medicinska sestra", LocationId = locations["Ambulanta Omiš"], IsActive = true },
            new Employee { FirstName = "Damir", LastName = "Lovrić", Email = "damir.lovric@skafetin.hr", Phone = "021 880 501", JobTitle = "Domar", LocationId = locations["Dom za starije Trogir"], IsActive = true },
            new Employee { FirstName = "Sanja", LastName = "Grubišić", Email = "sanja.grubisic@skafetin.hr", Phone = "021 880 502", JobTitle = "Socijalna radnica", LocationId = locations["Dom za starije Trogir"], IsActive = true },
            new Employee { FirstName = "Ivan", LastName = "Delić", Email = "ivan.delic@skafetin.hr", Phone = null, JobTitle = "Referent nabave", LocationId = locations["Županijska uprava"], IsActive = false }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedEquipmentAsync(SkafetinDbContext db)
    {
        if (await db.Equipment.AnyAsync())
            return;

        var locations = await db.Locations.ToDictionaryAsync(loc => loc.Name, loc => loc.Id);

        // Kategorije i statusi dolaze iz šifrarnika (HasData u SkafetinDbContext):
        // kategorije: 1 Računalna oprema, 2 Mrežna oprema, 3 Namještaj, 4 Alat, 5 Klimatizacija, 6 Medicinska oprema, 7 Ostalo
        // statusi:    1 Na skladištu, 2 Zaduženo, 3 Na servisu, 4 Nedostaje, 5 Otpisano
        // Svaki komad sa statusom Zaduženo dobiva aktivno zaduženje u SeedAssignmentsAsync.
        // Ako se ovdje doda ili makne komad sa statusom 2, mora se uskladiti i ondje.

        db.Equipment.AddRange(
            // Računalna oprema
            new Equipment { InventoryNumber = "INV-0001", Name = "Prijenosno računalo Dell Latitude 5540", Manufacturer = "Dell", Model = "Latitude 5540", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 1149.00m, CreatedAt = new DateTime(2023, 3, 14) },
            new Equipment { InventoryNumber = "INV-0002", Name = "Prijenosno računalo Lenovo ThinkPad T14", Manufacturer = "Lenovo", Model = "ThinkPad T14 Gen 4", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 1290.00m, CreatedAt = new DateTime(2023, 5, 9) },
            new Equipment { InventoryNumber = "INV-0003", Name = "Prijenosno računalo HP ProBook 450", Manufacturer = "HP", Model = "ProBook 450 G10", EquipmentCategoryId = 1, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 899.00m, CreatedAt = new DateTime(2024, 1, 22) },
            new Equipment { InventoryNumber = "INV-0004", Name = "Stolno računalo HP EliteDesk 800", Manufacturer = "HP", Model = "EliteDesk 800 G9", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 749.00m, CreatedAt = new DateTime(2022, 11, 3) },
            new Equipment { InventoryNumber = "INV-0005", Name = "Stolno računalo Lenovo ThinkCentre M70q", Manufacturer = "Lenovo", Model = "ThinkCentre M70q", EquipmentCategoryId = 1, EquipmentStatusId = 3, LocationId = locations["OŠ Kamen-Šine"], PurchaseValue = 685.00m, CreatedAt = new DateTime(2022, 9, 18), Description = "Ne pokreće se, poslano u servis." },
            new Equipment { InventoryNumber = "INV-0006", Name = "Monitor Dell P2422H 24\"", Manufacturer = "Dell", Model = "P2422H", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 179.00m, CreatedAt = new DateTime(2023, 3, 14) },
            new Equipment { InventoryNumber = "INV-0007", Name = "Monitor Dell P2422H 24\"", Manufacturer = "Dell", Model = "P2422H", EquipmentCategoryId = 1, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 179.00m, CreatedAt = new DateTime(2023, 3, 14) },
            new Equipment { InventoryNumber = "INV-0008", Name = "Monitor Philips 243V7 24\"", Manufacturer = "Philips", Model = "243V7QDSB", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["SŠ Braće Radić"], PurchaseValue = 129.00m, CreatedAt = new DateTime(2024, 2, 6) },
            new Equipment { InventoryNumber = "INV-0009", Name = "Pisač HP LaserJet Pro M404dn", Manufacturer = "HP", Model = "LaserJet Pro M404dn", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 289.00m, CreatedAt = new DateTime(2022, 6, 30) },
            new Equipment { InventoryNumber = "INV-0010", Name = "Multifunkcijski uređaj Canon i-SENSYS MF443dw", Manufacturer = "Canon", Model = "i-SENSYS MF443dw", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["Dom zdravlja Solin"], PurchaseValue = 419.00m, CreatedAt = new DateTime(2023, 10, 11) },
            new Equipment { InventoryNumber = "INV-0011", Name = "Skener Epson WorkForce DS-770", Manufacturer = "Epson", Model = "DS-770 II", EquipmentCategoryId = 1, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 649.00m, CreatedAt = new DateTime(2024, 4, 3) },
            new Equipment { InventoryNumber = "INV-0012", Name = "Projektor Epson EB-W52", Manufacturer = "Epson", Model = "EB-W52", EquipmentCategoryId = 1, EquipmentStatusId = 2, LocationId = locations["OŠ Kamen-Šine"], PurchaseValue = 529.00m, CreatedAt = new DateTime(2023, 8, 27) },
            new Equipment { InventoryNumber = "INV-0013", Name = "Tablet Samsung Galaxy Tab A9+", Manufacturer = "Samsung", Model = "Galaxy Tab A9+", EquipmentCategoryId = 1, EquipmentStatusId = 4, LocationId = locations["SŠ Braće Radić"], PurchaseValue = 239.00m, CreatedAt = new DateTime(2024, 9, 16), Description = "Nije pronađen na posljednjoj inventuri." },
            new Equipment { InventoryNumber = "INV-0014", Name = "Stolno računalo Fujitsu Esprimo P556", Manufacturer = "Fujitsu", Model = "Esprimo P556", EquipmentCategoryId = 1, EquipmentStatusId = 5, LocationId = locations["Staro skladište Dugopolje"], PurchaseValue = 540.00m, CreatedAt = new DateTime(2016, 5, 20), Description = "Otpisano zbog dotrajalosti." },

            // Mrežna oprema
            new Equipment { InventoryNumber = "INV-0101", Name = "Preklopnik Cisco Catalyst 1000 24-port", Manufacturer = "Cisco", Model = "C1000-24T", EquipmentCategoryId = 2, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 989.00m, CreatedAt = new DateTime(2022, 4, 8) },
            new Equipment { InventoryNumber = "INV-0102", Name = "Preklopnik TP-Link TL-SG1016D", Manufacturer = "TP-Link", Model = "TL-SG1016D", EquipmentCategoryId = 2, EquipmentStatusId = 2, LocationId = locations["OŠ Kamen-Šine"], PurchaseValue = 79.00m, CreatedAt = new DateTime(2023, 1, 30) },
            new Equipment { InventoryNumber = "INV-0103", Name = "Usmjerivač MikroTik hEX S", Manufacturer = "MikroTik", Model = "RB760iGS", EquipmentCategoryId = 2, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 69.00m, CreatedAt = new DateTime(2024, 3, 12) },
            new Equipment { InventoryNumber = "INV-0104", Name = "Pristupna točka Ubiquiti UniFi U6 Lite", Manufacturer = "Ubiquiti", Model = "U6-Lite", EquipmentCategoryId = 2, EquipmentStatusId = 2, LocationId = locations["Dom za starije Trogir"], PurchaseValue = 109.00m, CreatedAt = new DateTime(2024, 5, 21) },
            new Equipment { InventoryNumber = "INV-0105", Name = "Pristupna točka Ubiquiti UniFi U6 Lite", Manufacturer = "Ubiquiti", Model = "U6-Lite", EquipmentCategoryId = 2, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 109.00m, CreatedAt = new DateTime(2024, 5, 21) },
            new Equipment { InventoryNumber = "INV-0106", Name = "Neprekidno napajanje APC Smart-UPS 1500VA", Manufacturer = "APC", Model = "SMT1500IC", EquipmentCategoryId = 2, EquipmentStatusId = 3, LocationId = locations["Županijska uprava"], PurchaseValue = 749.00m, CreatedAt = new DateTime(2021, 12, 2), Description = "Zamjena baterije u tijeku." },

            // Namještaj
            new Equipment { InventoryNumber = "INV-0201", Name = "Uredski stol 160x80", Manufacturer = "Meblo", Model = "Linea 160", EquipmentCategoryId = 3, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 219.00m, CreatedAt = new DateTime(2021, 7, 15) },
            new Equipment { InventoryNumber = "INV-0202", Name = "Uredski stol 160x80", Manufacturer = "Meblo", Model = "Linea 160", EquipmentCategoryId = 3, EquipmentStatusId = 2, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 219.00m, CreatedAt = new DateTime(2021, 7, 15) },
            new Equipment { InventoryNumber = "INV-0203", Name = "Uredska stolica Ergo Plus", Manufacturer = "Sedia", Model = "Ergo Plus", EquipmentCategoryId = 3, EquipmentStatusId = 2, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 189.00m, CreatedAt = new DateTime(2022, 2, 24) },
            new Equipment { InventoryNumber = "INV-0204", Name = "Uredska stolica Ergo Plus", Manufacturer = "Sedia", Model = "Ergo Plus", EquipmentCategoryId = 3, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 189.00m, CreatedAt = new DateTime(2022, 2, 24) },
            new Equipment { InventoryNumber = "INV-0205", Name = "Ormar za spise 180x80", Manufacturer = "Meblo", Model = "Arhiv 180", EquipmentCategoryId = 3, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 349.00m, CreatedAt = new DateTime(2020, 10, 5) },
            new Equipment { InventoryNumber = "INV-0206", Name = "Konferencijski stol 240x100", Manufacturer = "Meblo", Model = "Forum 240", EquipmentCategoryId = 3, EquipmentStatusId = 2, LocationId = locations["SŠ Braće Radić"], PurchaseValue = 629.00m, CreatedAt = new DateTime(2021, 3, 11) },
            new Equipment { InventoryNumber = "INV-0207", Name = "Bolnički krevet na kotačima", Manufacturer = "Linet", Model = "Eleganza 1", EquipmentCategoryId = 3, EquipmentStatusId = 2, LocationId = locations["Dom za starije Trogir"], PurchaseValue = 1450.00m, CreatedAt = new DateTime(2022, 8, 19) },
            new Equipment { InventoryNumber = "INV-0208", Name = "Metalni regal 200x100", Manufacturer = "Bito", Model = "SR 2010", EquipmentCategoryId = 3, EquipmentStatusId = 5, LocationId = locations["Staro skladište Dugopolje"], PurchaseValue = 159.00m, CreatedAt = new DateTime(2015, 6, 1), Description = "Otpisano, korozija nosača." },

            // Alat
            new Equipment { InventoryNumber = "INV-0301", Name = "Udarna bušilica Bosch GSB 18V-55", Manufacturer = "Bosch", Model = "GSB 18V-55", EquipmentCategoryId = 4, EquipmentStatusId = 2, LocationId = locations["Dom za starije Trogir"], PurchaseValue = 199.00m, CreatedAt = new DateTime(2023, 6, 14) },
            new Equipment { InventoryNumber = "INV-0302", Name = "Set ručnog alata 108 dijelova", Manufacturer = "Stanley", Model = "STMT98109", EquipmentCategoryId = 4, EquipmentStatusId = 2, LocationId = locations["Dom zdravlja Solin"], PurchaseValue = 149.00m, CreatedAt = new DateTime(2023, 6, 14) },
            new Equipment { InventoryNumber = "INV-0303", Name = "Aluminijske ljestve 8 stepenica", Manufacturer = "Krause", Model = "Corda 8", EquipmentCategoryId = 4, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 119.00m, CreatedAt = new DateTime(2022, 5, 27) },
            new Equipment { InventoryNumber = "INV-0304", Name = "Kutna brusilica Makita GA5030", Manufacturer = "Makita", Model = "GA5030", EquipmentCategoryId = 4, EquipmentStatusId = 4, LocationId = locations["Dom za starije Trogir"], PurchaseValue = 89.00m, CreatedAt = new DateTime(2021, 9, 8), Description = "Nije pronađena na inventuri 2025." },
            new Equipment { InventoryNumber = "INV-0305", Name = "Kosilica Husqvarna LC 140", Manufacturer = "Husqvarna", Model = "LC 140", EquipmentCategoryId = 4, EquipmentStatusId = 3, LocationId = locations["OŠ Kamen-Šine"], PurchaseValue = 379.00m, CreatedAt = new DateTime(2022, 4, 19), Description = "Servis motora." },

            // Klimatizacija
            new Equipment { InventoryNumber = "INV-0401", Name = "Klima uređaj Daikin Sensira 3,5 kW", Manufacturer = "Daikin", Model = "FTXF35E", EquipmentCategoryId = 5, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 799.00m, CreatedAt = new DateTime(2023, 5, 4) },
            new Equipment { InventoryNumber = "INV-0402", Name = "Klima uređaj Mitsubishi MSZ-HR35 3,5 kW", Manufacturer = "Mitsubishi Electric", Model = "MSZ-HR35VF", EquipmentCategoryId = 5, EquipmentStatusId = 2, LocationId = locations["Dom zdravlja Solin"], PurchaseValue = 749.00m, CreatedAt = new DateTime(2023, 5, 4) },
            new Equipment { InventoryNumber = "INV-0403", Name = "Klima uređaj Vivax ACP-12 3,5 kW", Manufacturer = "Vivax", Model = "ACP-12CH35AERI", EquipmentCategoryId = 5, EquipmentStatusId = 3, LocationId = locations["Ambulanta Omiš"], PurchaseValue = 429.00m, CreatedAt = new DateTime(2022, 7, 12), Description = "Curi kondenzat, prijavljen servis." },
            new Equipment { InventoryNumber = "INV-0404", Name = "Prijenosni ovlaživač zraka Boneco H300", Manufacturer = "Boneco", Model = "H300", EquipmentCategoryId = 5, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 289.00m, CreatedAt = new DateTime(2024, 11, 8) },

            // Medicinska oprema
            new Equipment { InventoryNumber = "INV-0501", Name = "Tlakomjer Omron M7 Intelli IT", Manufacturer = "Omron", Model = "M7 Intelli IT", EquipmentCategoryId = 6, EquipmentStatusId = 2, LocationId = locations["Ambulanta Omiš"], PurchaseValue = 119.00m, CreatedAt = new DateTime(2024, 1, 15) },
            new Equipment { InventoryNumber = "INV-0502", Name = "EKG uređaj Schiller Cardiovit AT-1", Manufacturer = "Schiller", Model = "Cardiovit AT-1 G2", EquipmentCategoryId = 6, EquipmentStatusId = 2, LocationId = locations["Dom zdravlja Solin"], PurchaseValue = 3290.00m, CreatedAt = new DateTime(2022, 10, 26) },
            new Equipment { InventoryNumber = "INV-0503", Name = "Defibrilator Zoll AED Plus", Manufacturer = "Zoll", Model = "AED Plus", EquipmentCategoryId = 6, EquipmentStatusId = 2, LocationId = locations["Dom zdravlja Solin"], PurchaseValue = 1890.00m, CreatedAt = new DateTime(2023, 2, 9) },
            new Equipment { InventoryNumber = "INV-0504", Name = "Invalidska kolica Meyra Eurochair", Manufacturer = "Meyra", Model = "Eurochair 2", EquipmentCategoryId = 6, EquipmentStatusId = 1, LocationId = locations["Dom za starije Trogir"], PurchaseValue = 549.00m, CreatedAt = new DateTime(2023, 11, 20) },
            new Equipment { InventoryNumber = "INV-0505", Name = "Inhalator Omron C102 Total", Manufacturer = "Omron", Model = "C102 Total", EquipmentCategoryId = 6, EquipmentStatusId = 5, LocationId = locations["Staro skladište Dugopolje"], PurchaseValue = 69.00m, CreatedAt = new DateTime(2017, 3, 7), Description = "Otpisano, istekao vijek trajanja." },

            // Ostalo
            new Equipment { InventoryNumber = "INV-0601", Name = "Hladnjak Gorenje RB615FEW5", Manufacturer = "Gorenje", Model = "RB615FEW5", EquipmentCategoryId = 7, EquipmentStatusId = 2, LocationId = locations["Dom za starije Trogir"], PurchaseValue = 349.00m, CreatedAt = new DateTime(2022, 12, 1) },
            new Equipment { InventoryNumber = "INV-0602", Name = "Aparat za kavu Jura E8", Manufacturer = "Jura", Model = "E8", EquipmentCategoryId = 7, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 1099.00m, CreatedAt = new DateTime(2023, 9, 5) },
            new Equipment { InventoryNumber = "INV-0603", Name = "Uništavač dokumenata Fellowes 79Ci", Manufacturer = "Fellowes", Model = "Powershred 79Ci", EquipmentCategoryId = 7, EquipmentStatusId = 1, LocationId = locations["Upravni odjel za imovinu"], PurchaseValue = 329.00m, CreatedAt = new DateTime(2024, 6, 18) },
            new Equipment { InventoryNumber = "INV-0604", Name = "Službeno vozilo Škoda Octavia", Manufacturer = "Škoda", Model = "Octavia 2.0 TDI", EquipmentCategoryId = 7, EquipmentStatusId = 2, LocationId = locations["Županijska uprava"], PurchaseValue = 24900.00m, CreatedAt = new DateTime(2021, 5, 13) }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedAssignmentsAsync(SkafetinDbContext db)
    {
        if (await db.Assignments.AnyAsync())
            return;

        var equipment = await db.Equipment.ToDictionaryAsync(e => e.InventoryNumber, e => e.Id);
        var employees = await db.Employees.ToDictionaryAsync(emp => emp.Email, emp => emp.Id);

        // Statusi zaduženja (HasData): 1 Aktivno, 2 Vraćeno, 3 Premješteno, 4 Stornirano.
        const int active = 1;
        const int returned = 2;
        const int transferred = 3;
        const int canceled = 4;

        Assignment New(string inventoryNumber, string email, DateTime assignedAt, int statusId,
                       DateTime? returnedAt = null, string? note = null, int? previousId = null) =>
            new()
            {
                EquipmentId = equipment[inventoryNumber],
                EmployeeId = employees[email],
                AssignedAt = assignedAt,
                ReturnedAt = returnedAt,
                AssignmentStatusId = statusId,
                PreviousAssignmentId = previousId,
                Note = note,
                CreatedAt = assignedAt
            };

        // 1. Aktivna zaduženja - točno jedno za svaki komad sa statusom Zaduženo,
        //    osim INV-0002 koji aktivno zaduženje dobiva na kraju lanca prijenosa.
        db.Assignments.AddRange(
            // Županijska uprava
            New("INV-0001", "ivana.barisic@skafetin.hr", new DateTime(2025, 1, 13), active),
            New("INV-0004", "ivana.barisic@skafetin.hr", new DateTime(2024, 9, 2), active),
            New("INV-0006", "ivana.barisic@skafetin.hr", new DateTime(2025, 1, 13), active, note: "Drugi monitor uz prijenosno računalo."),
            New("INV-0201", "ivana.barisic@skafetin.hr", new DateTime(2024, 9, 2), active),
            New("INV-0205", "ivana.barisic@skafetin.hr", new DateTime(2024, 9, 2), active),
            New("INV-0401", "ivana.barisic@skafetin.hr", new DateTime(2025, 5, 19), active),
            New("INV-0602", "ivana.barisic@skafetin.hr", new DateTime(2025, 3, 3), active, note: "Zajednička kuhinja, zadužena na pročelnicu."),
            New("INV-0101", "marko.juric@skafetin.hr", new DateTime(2024, 4, 8), active, note: "Zadužen na voditelja imovine, smješten u serverskoj sobi."),
            New("INV-0604", "marko.juric@skafetin.hr", new DateTime(2025, 2, 10), active, note: "Službeno vozilo, ključevi i prometna kod voditelja imovine."),

            // Upravni odjel za imovinu
            New("INV-0009", "petra.kovacevic@skafetin.hr", new DateTime(2024, 10, 14), active),
            New("INV-0203", "petra.kovacevic@skafetin.hr", new DateTime(2024, 10, 14), active),
            New("INV-0202", "marko.juric@skafetin.hr", new DateTime(2024, 3, 25), active),

            // OŠ Kamen-Šine
            New("INV-0012", "josip.matic@skafetin.hr", new DateTime(2025, 9, 8), active, note: "Informatička učionica."),
            New("INV-0102", "josip.matic@skafetin.hr", new DateTime(2025, 9, 8), active),

            // SŠ Braće Radić
            New("INV-0008", "maja.saric@skafetin.hr", new DateTime(2025, 4, 7), active),
            New("INV-0206", "maja.saric@skafetin.hr", new DateTime(2024, 11, 4), active, note: "Zbornica."),

            // Dom zdravlja Solin
            New("INV-0010", "luka.bilic@skafetin.hr", new DateTime(2024, 12, 2), active),
            New("INV-0302", "luka.bilic@skafetin.hr", new DateTime(2025, 6, 16), active),
            New("INV-0402", "luka.bilic@skafetin.hr", new DateTime(2025, 5, 26), active),
            New("INV-0502", "luka.bilic@skafetin.hr", new DateTime(2024, 8, 19), active, note: "Ordinacija 3."),
            New("INV-0503", "luka.bilic@skafetin.hr", new DateTime(2024, 8, 19), active, note: "Hodnik uz čekaonicu."),

            // Ambulanta Omiš
            New("INV-0501", "nikolina.radic@skafetin.hr", new DateTime(2025, 2, 24), active),

            // Dom za starije Trogir
            New("INV-0104", "damir.lovric@skafetin.hr", new DateTime(2025, 3, 17), active),
            New("INV-0301", "damir.lovric@skafetin.hr", new DateTime(2025, 1, 20), active),
            New("INV-0601", "damir.lovric@skafetin.hr", new DateTime(2024, 10, 7), active, note: "Čajna kuhinja prvog kata."),
            New("INV-0207", "sanja.grubisic@skafetin.hr", new DateTime(2025, 4, 14), active, note: "Soba 12."),

            // 2. Zatvorena zaduženja - oprema je u međuvremenu vraćena, status opreme je Na skladištu.
            New("INV-0003", "petra.kovacevic@skafetin.hr", new DateTime(2024, 2, 5), returned,
                returnedAt: new DateTime(2025, 11, 17), note: "Zamijenjeno novijim modelom."),
            New("INV-0007", "tomislav.vukovic@skafetin.hr", new DateTime(2024, 6, 3), returned,
                returnedAt: new DateTime(2026, 1, 12)),

            // 3. Stornirano zaduženje - uneseno greškom, zapis ostaje u povijesti (pravilo 13).
            //    Razlog storna bit će zaseban stupac; do tada stoji u napomeni (docs/07, stavka 1).
            New("INV-0011", "tomislav.vukovic@skafetin.hr", new DateTime(2025, 10, 6), canceled,
                returnedAt: new DateTime(2025, 10, 6), note: "Storno: zaduženje uneseno na krivi inventurni broj."),

            // 4. Povijest opreme koja trenutno nije zadužena. Sva ova zaduženja su zatvorena -
            //    aktivno zaduženje smije imati samo oprema sa statusom Zaduženo.

            // Na skladištu - vraćeno i čeka sljedeće zaduženje
            New("INV-0103", "marko.juric@skafetin.hr", new DateTime(2024, 4, 2), returned,
                returnedAt: new DateTime(2025, 10, 20), note: "Zamijenjen novijim uređajem."),
            New("INV-0204", "tomislav.vukovic@skafetin.hr", new DateTime(2022, 3, 7), returned,
                returnedAt: new DateTime(2025, 7, 4)),
            New("INV-0504", "sanja.grubisic@skafetin.hr", new DateTime(2023, 12, 4), returned,
                returnedAt: new DateTime(2026, 2, 16), note: "Korisnik otpušten iz doma."),
            New("INV-0603", "petra.kovacevic@skafetin.hr", new DateTime(2024, 7, 1), returned,
                returnedAt: new DateTime(2025, 9, 15)),

            // Ista oprema zaduživana dvaput - profil opreme pokazuje više redaka
            New("INV-0303", "damir.lovric@skafetin.hr", new DateTime(2022, 6, 13), returned,
                returnedAt: new DateTime(2023, 5, 19), note: "Posudba za radove u domu."),
            New("INV-0303", "luka.bilic@skafetin.hr", new DateTime(2024, 3, 4), returned,
                returnedAt: new DateTime(2024, 9, 27), note: "Posudba za sanaciju krova."),

            // Na servisu - oprema je vraćena, pa poslana u servis
            New("INV-0005", "ana.peric@skafetin.hr", new DateTime(2022, 10, 3), returned,
                returnedAt: new DateTime(2026, 4, 13), note: "Vraćeno jer se ne pokreće, poslano u servis."),
            New("INV-0106", "ivana.barisic@skafetin.hr", new DateTime(2021, 12, 20), returned,
                returnedAt: new DateTime(2026, 5, 11), note: "Vraćeno radi zamjene baterije."),
            New("INV-0305", "josip.matic@skafetin.hr", new DateTime(2022, 5, 9), returned,
                returnedAt: new DateTime(2026, 3, 2), note: "Vraćeno radi servisa motora."),
            New("INV-0403", "nikolina.radic@skafetin.hr", new DateTime(2022, 8, 1), returned,
                returnedAt: new DateTime(2026, 6, 8), note: "Vraćeno, curi kondenzat."),

            // Nedostaje - zadnji poznati korisnik ostaje vidljiv u povijesti
            New("INV-0013", "maja.saric@skafetin.hr", new DateTime(2024, 10, 7), returned,
                returnedAt: new DateTime(2026, 1, 26), note: "Nije pronađen na inventuri, evidentirano kao manjak."),
            New("INV-0304", "damir.lovric@skafetin.hr", new DateTime(2021, 9, 27), returned,
                returnedAt: new DateTime(2025, 11, 10), note: "Nije pronađena na inventuri 2025."),

            // Otpisano - zaduženja iz radnog vijeka opreme, zapisi ostaju (pravilo 13)
            New("INV-0014", "ivan.delic@skafetin.hr", new DateTime(2016, 6, 6), returned,
                returnedAt: new DateTime(2023, 4, 18), note: "Vraćeno pri odlasku zaposlenika, oprema kasnije otpisana."),
            New("INV-0505", "luka.bilic@skafetin.hr", new DateTime(2017, 3, 20), returned,
                returnedAt: new DateTime(2024, 5, 7), note: "Istekao vijek trajanja.")

            // INV-0105, INV-0404 i INV-0208 namjerno ostaju bez ijednog zaduženja,
            // da se na profilu opreme vidi i prazno stanje povijesti.
        );

        await db.SaveChangesAsync();

        // 5. Lanac prijenosa za INV-0002: tri zapisa, dva Premješteno i jedan aktivan.
        //    Sprema se u koracima jer PreviousAssignmentId traži Id prethodnog zapisa.
        var first = New("INV-0002", "marko.juric@skafetin.hr", new DateTime(2024, 1, 15), transferred,
            returnedAt: new DateTime(2025, 2, 3), note: "Prvo zaduženje nakon nabave.");
        db.Assignments.Add(first);
        await db.SaveChangesAsync();

        var second = New("INV-0002", "petra.kovacevic@skafetin.hr", new DateTime(2025, 2, 3), transferred,
            returnedAt: new DateTime(2025, 12, 9), previousId: first.Id, note: "Preuzeto zbog preraspodjele poslova.");
        db.Assignments.Add(second);
        await db.SaveChangesAsync();

        var third = New("INV-0002", "tomislav.vukovic@skafetin.hr", new DateTime(2025, 12, 9), active,
            previousId: second.Id, note: "Preuzeto za rad u skladištu.");
        db.Assignments.Add(third);
        await db.SaveChangesAsync();
    }

    private static async Task SeedInventoriesAsync(SkafetinDbContext db)
    {
        if (await db.Inventories.AnyAsync())
            return;

        var locations = await db.Locations.ToDictionaryAsync(loc => loc.Name, loc => loc.Id);
        var employees = await db.Employees.ToDictionaryAsync(emp => emp.Email, emp => emp.Id);

        // Statusi inventure (HasData): 1 Skica, 2 Otvorena, 3 U tijeku, 4 Završena, 5 Zaključana.
        const int draft = 1;
        const int inProgress = 3;
        const int locked = 5;

        // Otpisana oprema (status 5) ne ulazi u stavke - isto pravilo kao u InventoriesController.
        const int writtenOff = 5;
        const int assignmentActive = 1;

        // Očekivani zaposlenik je onaj koji opremu ima aktivno zaduženu, kao pri /open.
        var activeAssignments = await db.Assignments
            .Where(a => a.AssignmentStatusId == assignmentActive)
            .ToDictionaryAsync(a => a.EquipmentId, a => a.EmployeeId);

        async Task<List<InventoryItem>> ItemsForLocationAsync(int locationId)
        {
            var equipment = await db.Equipment
                .Where(e => e.LocationId == locationId && e.EquipmentStatusId != writtenOff)
                .OrderBy(e => e.InventoryNumber)
                .Select(e => new { e.Id, e.InventoryNumber, e.LocationId })
                .ToListAsync();

            return equipment
                .Select(e => new InventoryItem
                {
                    EquipmentId = e.Id,
                    ExpectedLocationId = e.LocationId,
                    ExpectedEmployeeId = activeAssignments.TryGetValue(e.Id, out var employeeId)
                        ? employeeId
                        : null,
                    IsFound = null,
                    IsDamaged = false
                })
                .ToList();
        }

        var equipmentIdsByNumber = await db.Equipment
            .ToDictionaryAsync(e => e.InventoryNumber, e => e.Id);

        void Check(List<InventoryItem> items, string inventoryNumber, bool isFound, DateTime checkedAt,
                   int checkedByEmployeeId, bool isDamaged = false, int? foundLocationId = null, string? note = null)
        {
            var item = items.Single(x => x.EquipmentId == equipmentIdsByNumber[inventoryNumber]);
            item.IsFound = isFound;
            item.IsDamaged = isDamaged;
            item.FoundLocationId = isFound ? foundLocationId : null;
            item.Note = note;
            item.CheckedAt = checkedAt;
            item.CheckedByEmployeeId = checkedByEmployeeId;
        }

        // 1. Zaključana inventura - konačno stanje, služi za provjeru pravila 11 (svaka izmjena vraća 400).
        //    Manjak INV-0013 objašnjava zašto ta oprema ima status Nedostaje u SeedEquipmentAsync.
        var majaId = employees["maja.saric@skafetin.hr"];
        var closedItems = await ItemsForLocationAsync(locations["SŠ Braće Radić"]);
        Check(closedItems, "INV-0008", true, new DateTime(2026, 1, 22), majaId,
              foundLocationId: locations["SŠ Braće Radić"]);
        Check(closedItems, "INV-0013", false, new DateTime(2026, 1, 22), majaId,
              note: "Nije pronađen ni u zbornici ni u učionici.");
        Check(closedItems, "INV-0206", true, new DateTime(2026, 1, 23), majaId,
              foundLocationId: locations["SŠ Braće Radić"]);

        var closed = new Inventory
        {
            Code = "INV-2026-001",
            LocationId = locations["SŠ Braće Radić"],
            InventoryStatusId = locked,
            CreatedByEmployeeId = employees["marko.juric@skafetin.hr"],
            CreatedAt = new DateTime(2026, 1, 20),
            StartedAt = new DateTime(2026, 1, 21),
            CompletedAt = new DateTime(2026, 1, 23),
            LockedAt = new DateTime(2026, 1, 26),
            Note = "Redovna godišnja inventura.",
            InventoryItems = closedItems
        };

        // 2. Inventura u tijeku - dio stavaka popisan, dio još nije. Lokacija je OŠ Kamen-Šine,
        //    čija je odgovorna osoba ana.peric, pa se na njoj provjerava i pravilo 10 (403).
        var anaId = employees["ana.peric@skafetin.hr"];
        var runningItems = await ItemsForLocationAsync(locations["OŠ Kamen-Šine"]);
        Check(runningItems, "INV-0005", true, new DateTime(2026, 9, 1), anaId,
              isDamaged: true, foundLocationId: locations["OŠ Kamen-Šine"],
              note: "Kućište oštećeno, uređaj se ne pokreće.");
        Check(runningItems, "INV-0012", true, new DateTime(2026, 9, 1), anaId,
              foundLocationId: locations["OŠ Kamen-Šine"]);
        Check(runningItems, "INV-0102", true, new DateTime(2026, 9, 2), anaId,
              foundLocationId: locations["Županijska uprava"],
              note: "Pronađen u serverskoj sobi županijske uprave.");
        // INV-0305 namjerno ostaje nepopisan, da se u sažetku vidi razlika Counted / Total.

        var running = new Inventory
        {
            Code = "INV-2026-002",
            LocationId = locations["OŠ Kamen-Šine"],
            InventoryStatusId = inProgress,
            CreatedByEmployeeId = anaId,
            CreatedAt = new DateTime(2026, 8, 28),
            StartedAt = new DateTime(2026, 8, 31),
            Note = "Inventura prije početka školske godine.",
            InventoryItems = runningItems
        };

        // 3. Skica bez stavaka - stavke nastaju tek pozivom /open (pravilo 12).
        var draftInventory = new Inventory
        {
            Code = "INV-2026-003",
            LocationId = locations["Dom zdravlja Solin"],
            InventoryStatusId = draft,
            CreatedByEmployeeId = employees["marko.juric@skafetin.hr"],
            CreatedAt = new DateTime(2026, 9, 2),
            Note = "Priprema za jesensku inventuru."
        };

        db.Inventories.AddRange(closed, running, draftInventory);
        await db.SaveChangesAsync();
    }
}

