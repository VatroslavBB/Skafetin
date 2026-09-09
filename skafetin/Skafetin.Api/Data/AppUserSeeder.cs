using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Security;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Data;

public static class AppUserSeeder
{
    public const string DemoPassword = "Skafetin1!";

    private sealed record DemoUser(string Username, string EmployeeEmail, string[] Roles);

    private static readonly DemoUser[] DemoUsers =
    [
        new("admin", "ivana.barisic@skafetin.hr", ["Admin"]),

        new("marko.juric", "marko.juric@skafetin.hr", ["InventoryManager", "LocationResponsible"]),

        new("petra.kovacevic", "petra.kovacevic@skafetin.hr", ["InventoryManager"]),

        new("ana.peric", "ana.peric@skafetin.hr", ["LocationResponsible"]),

        new("josip.matic", "josip.matic@skafetin.hr", ["Employee"]),

        new("nikolina.radic", "nikolina.radic@skafetin.hr", ["Employee"])
    ];

    public static async Task SeedAsync(SkafetinDbContext db, ILogger? logger = null)
    {
        var roleIds = await db.AppRoles.ToDictionaryAsync(role => role.Name, role => role.Id);
        var employees = await db.Employees.ToDictionaryAsync(employee => employee.Email);
        var existingUsernames = await db.AppUsers.Select(user => user.Username).ToListAsync();

        var added = false;

        foreach (var demo in DemoUsers)
        {
            // Provjera ide po računu, a ne nad cijelom tablicom, da se novi demo račun
            // doda i u bazu koja već ima korisnike.
            if (existingUsernames.Contains(demo.Username))
                continue;

            if (!employees.TryGetValue(demo.EmployeeEmail, out var employee))
            {
                logger?.LogWarning(
                    "Demo račun {Username} nije stvoren jer zaposlenik {Email} ne postoji.",
                    demo.Username,
                    demo.EmployeeEmail);
                continue;
            }

            var (hash, salt) = PasswordHasher.Create(DemoPassword);

            var user = new AppUser
            {
                Username = demo.Username,
                Email = employee.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                EmployeeId = employee.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var roleName in demo.Roles)
                user.UserRoles.Add(new AppUserRole { AppRoleId = roleIds[roleName] });

            db.AppUsers.Add(user);
            added = true;
        }

        if (added)
            await db.SaveChangesAsync();
    }
}
