using Skafetin.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Skafetin.Api.Data;

public class SkafetinDbContext : DbContext
{
    public SkafetinDbContext(DbContextOptions<SkafetinDbContext> options) : base(options)
    {
    }

    public DbSet<LocationType> LocationTypes => Set<LocationType>();
    public DbSet<EquipmentCategory> EquipmentCategories => Set<EquipmentCategory>();
    public DbSet<EquipmentStatus> EquipmentStatuses => Set<EquipmentStatus>();
    public DbSet<AssignmentStatus> AssignmentStatuses => Set<AssignmentStatus>();
    public DbSet<InventoryStatus> InventoryStatuses => Set<InventoryStatus>();
    public DbSet<RequestStatus> RequestStatuses => Set<RequestStatus>();
    public DbSet<WriteOffRequestStatus> WriteOffRequestStatuses => Set<WriteOffRequestStatus>();

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<EquipmentRequest> EquipmentRequests => Set<EquipmentRequest>();
    public DbSet<WriteOffRequest> WriteOffRequests => Set<WriteOffRequest>();
    public DbSet<EquipmentMedia> EquipmentMedia => Set<EquipmentMedia>();
    public DbSet<EquipmentStatusHistory> EquipmentStatusHistories => Set<EquipmentStatusHistory>();

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Equipment>().HasIndex(e => e.InventoryNumber).IsUnique();
        modelBuilder.Entity<Inventory>().HasIndex(i => i.Code).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(e => e.Email).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<AppRole>().HasIndex(r => r.Name).IsUnique();


        modelBuilder.Entity<Equipment>()
            .Property(e => e.PurchaseValue)
            .HasColumnType("decimal(18,2)");


        modelBuilder.Entity<Location>()
            .HasOne(l => l.LocationType)
            .WithMany(t => t.Locations)
            .HasForeignKey(l => l.LocationTypeId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Location)
            .WithMany(l => l.Employees)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Equipment>()
            .HasOne(e => e.EquipmentCategory)
            .WithMany(c => c.Equipment)
            .HasForeignKey(e => e.EquipmentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Equipment>()
            .HasOne(e => e.EquipmentStatus)
            .WithMany(s => s.Equipment)
            .HasForeignKey(e => e.EquipmentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Equipment>()
            .HasOne(e => e.Location)
            .WithMany(l => l.Equipment)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Equipment)
            .WithMany(e => e.Assignments)
            .HasForeignKey(a => a.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Assignments)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.AssignmentStatus)
            .WithMany(s => s.Assignments)
            .HasForeignKey(a => a.AssignmentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.PreviousAssignment)
            .WithMany()
            .HasForeignKey(a => a.PreviousAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Location)
            .WithMany(l => l.Inventories)
            .HasForeignKey(i => i.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.InventoryStatus)
            .WithMany(s => s.Inventories)
            .HasForeignKey(i => i.InventoryStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(i => i.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<InventoryItem>()
            .HasOne(item => item.Inventory)
            .WithMany(i => i.InventoryItems)
            .HasForeignKey(item => item.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(item => item.Equipment)
            .WithMany(e => e.InventoryItems)
            .HasForeignKey(item => item.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(item => item.ExpectedEmployee)
            .WithMany()
            .HasForeignKey(item => item.ExpectedEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(item => item.ExpectedLocation)
            .WithMany()
            .HasForeignKey(item => item.ExpectedLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(item => item.FoundLocation)
            .WithMany()
            .HasForeignKey(item => item.FoundLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(item => item.CheckedByEmployee)
            .WithMany()
            .HasForeignKey(item => item.CheckedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<EquipmentRequest>()
            .HasOne(r => r.RequestedByEmployee)
            .WithMany()
            .HasForeignKey(r => r.RequestedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentRequest>()
            .HasOne(r => r.EquipmentCategory)
            .WithMany()
            .HasForeignKey(r => r.EquipmentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentRequest>()
            .HasOne(r => r.RequestStatus)
            .WithMany(s => s.EquipmentRequests)
            .HasForeignKey(r => r.RequestStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentRequest>()
            .HasOne(r => r.ReplacementForEquipment)
            .WithMany()
            .HasForeignKey(r => r.ReplacementForEquipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EquipmentRequest>()
            .HasOne(r => r.ProcessedByEmployee)
            .WithMany()
            .HasForeignKey(r => r.ProcessedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EquipmentRequest>()
            .HasOne(r => r.ResultingEquipment)
            .WithMany()
            .HasForeignKey(r => r.ResultingEquipmentId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<WriteOffRequest>()
            .HasOne(w => w.Equipment)
            .WithMany()
            .HasForeignKey(w => w.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WriteOffRequest>()
            .HasOne(w => w.RequestedByEmployee)
            .WithMany()
            .HasForeignKey(w => w.RequestedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WriteOffRequest>()
            .HasOne(w => w.WriteOffRequestStatus)
            .WithMany(s => s.WriteOffRequests)
            .HasForeignKey(w => w.WriteOffRequestStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WriteOffRequest>()
            .HasOne(w => w.ProcessedByEmployee)
            .WithMany()
            .HasForeignKey(w => w.ProcessedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<EquipmentMedia>()
            .HasOne(m => m.Equipment)
            .WithMany(e => e.Media)
            .HasForeignKey(m => m.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EquipmentMedia>()
            .HasOne(m => m.UploadedByEmployee)
            .WithMany()
            .HasForeignKey(m => m.UploadedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<EquipmentStatusHistory>()
            .HasOne(h => h.Equipment)
            .WithMany()
            .HasForeignKey(h => h.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentStatusHistory>()
            .HasOne(h => h.FromStatus)
            .WithMany()
            .HasForeignKey(h => h.FromStatusId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EquipmentStatusHistory>()
            .HasOne(h => h.ToStatus)
            .WithMany()
            .HasForeignKey(h => h.ToStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentStatusHistory>()
            .HasOne(h => h.FromLocation)
            .WithMany()
            .HasForeignKey(h => h.FromLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EquipmentStatusHistory>()
            .HasOne(h => h.ToLocation)
            .WithMany()
            .HasForeignKey(h => h.ToLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EquipmentStatusHistory>()
            .HasOne(h => h.ChangedByEmployee)
            .WithMany()
            .HasForeignKey(h => h.ChangedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<AppUserRole>()
            .HasKey(ur => new { ur.AppUserId, ur.AppRoleId });

        modelBuilder.Entity<AppUserRole>()
            .HasOne(ur => ur.AppUser)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppUserRole>()
            .HasOne(ur => ur.AppRole)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.AppRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Employee)
            .WithMany()
            .HasForeignKey(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<LocationType>().HasData(
            new LocationType { Id = 1, Name = "Ured" },
            new LocationType { Id = 2, Name = "Škola" },
            new LocationType { Id = 3, Name = "Zdravstvena ustanova" },
            new LocationType { Id = 4, Name = "Socijalna ustanova" }
        );

        modelBuilder.Entity<EquipmentCategory>().HasData(
            new EquipmentCategory { Id = 1, Name = "Računalna oprema" },
            new EquipmentCategory { Id = 2, Name = "Mrežna oprema" },
            new EquipmentCategory { Id = 3, Name = "Namještaj" },
            new EquipmentCategory { Id = 4, Name = "Alat" },
            new EquipmentCategory { Id = 5, Name = "Klimatizacija" },
            new EquipmentCategory { Id = 6, Name = "Medicinska oprema" },
            new EquipmentCategory { Id = 7, Name = "Ostalo" }
        );

        modelBuilder.Entity<EquipmentStatus>().HasData(
            new EquipmentStatus { Id = 1, Name = "Na skladištu" },
            new EquipmentStatus { Id = 2, Name = "Zaduženo" },
            new EquipmentStatus { Id = 3, Name = "Na servisu" },
            new EquipmentStatus { Id = 4, Name = "Nedostaje" },
            new EquipmentStatus { Id = 5, Name = "Otpisano" }
        );

        modelBuilder.Entity<AssignmentStatus>().HasData(
            new AssignmentStatus { Id = 1, Name = "Aktivno" },
            new AssignmentStatus { Id = 2, Name = "Vraćeno" },
            new AssignmentStatus { Id = 3, Name = "Premješteno" },
            new AssignmentStatus { Id = 4, Name = "Stornirano" }
        );

        modelBuilder.Entity<InventoryStatus>().HasData(
            new InventoryStatus { Id = 1, Name = "Skica" },
            new InventoryStatus { Id = 2, Name = "Otvorena" },
            new InventoryStatus { Id = 3, Name = "U tijeku" },
            new InventoryStatus { Id = 4, Name = "Završena" },
            new InventoryStatus { Id = 5, Name = "Zaključana" }
        );

        modelBuilder.Entity<RequestStatus>().HasData(
            new RequestStatus { Id = 1, Name = "Zaprimljeno" },
            new RequestStatus { Id = 2, Name = "U obradi" },
            new RequestStatus { Id = 3, Name = "Odobreno" },
            new RequestStatus { Id = 4, Name = "Odbijeno" },
            new RequestStatus { Id = 5, Name = "Realizirano" },
            new RequestStatus { Id = 6, Name = "Zatvoreno" }
        );

        modelBuilder.Entity<WriteOffRequestStatus>().HasData(
            new WriteOffRequestStatus { Id = 1, Name = "Zaprimljeno" },
            new WriteOffRequestStatus { Id = 2, Name = "U obradi" },
            new WriteOffRequestStatus { Id = 3, Name = "Odobreno" },
            new WriteOffRequestStatus { Id = 4, Name = "Odbijeno" },
            new WriteOffRequestStatus { Id = 5, Name = "Provedeno" }
        );

        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = 1, Name = "Admin" },
            new AppRole { Id = 2, Name = "AssetManager" },
            new AppRole { Id = 3, Name = "LocationResponsible" },
            new AppRole { Id = 4, Name = "Employee" }
        );
    }
}

