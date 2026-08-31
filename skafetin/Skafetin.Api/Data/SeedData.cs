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
        // Komadi sa statusom Zaduženo još nemaju pripadajuće zaduženje jer modul zaduženja
        // dolazi u fazi 7 - kad se ona napravi, seed zaduženja mora pokriti upravo te komade.

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
}

