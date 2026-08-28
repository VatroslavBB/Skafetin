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

    private static Task SeedLocationsAsync(SkafetinDbContext db) => Task.CompletedTask;
    private static Task SeedEmployeesAsync(SkafetinDbContext db) => Task.CompletedTask;
    private static Task SeedEquipmentAsync(SkafetinDbContext db) => Task.CompletedTask;
}

