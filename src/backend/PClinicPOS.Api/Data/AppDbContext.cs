using Microsoft.EntityFrameworkCore;
using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Tenant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        });

        mb.Entity<Branch>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.TenantId);
        });

        mb.Entity<Patient>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).IsRequired().HasMaxLength(200);
            e.Property(x => x.LastName).IsRequired().HasMaxLength(200);
            e.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(50);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PrimaryBranch).WithMany().HasForeignKey(x => x.PrimaryBranchId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.TenantId, x.PhoneNumber }).IsUnique();
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.PrimaryBranchId);
        });

        mb.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
            e.Property(x => x.Role).IsRequired().HasMaxLength(50);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        });

        mb.Entity<UserBranch>(e =>
        {
            e.ToTable("UserBranch"); // Match migration table name (EF default would be "UserBranches")
            e.HasKey(x => new { x.UserId, x.BranchId });
            e.HasOne(x => x.User).WithMany(u => u.UserBranches).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Appointment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.TenantId, x.BranchId, x.PatientId, x.StartAt }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });
    }
}
