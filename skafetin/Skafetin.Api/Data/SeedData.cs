using Skafetin.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Skafetin.Api.Data;
public static class SeedData
{
    public static async Task SeedAsync(SkafetinDbContext db)
    {
        await SeedLocationsAsync(db);
        await SeedEmployeesAsync(db);
        await SeedEquipmentAsync(db);
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

    private static Task SeedEquipmentAsync(SkafetinDbContext db) => Task.CompletedTask;
}

