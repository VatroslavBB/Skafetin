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

        await Task.CompletedTask;
    }
    private static Task SeedEmployeesAsync(SkafetinDbContext db) => Task.CompletedTask;
    private static Task SeedEquipmentAsync(SkafetinDbContext db) => Task.CompletedTask;
}

