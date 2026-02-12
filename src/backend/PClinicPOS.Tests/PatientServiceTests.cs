using Microsoft.EntityFrameworkCore;
using PClinicPOS.Api.Auth;
using PClinicPOS.Api.Data;
using PClinicPOS.Api.Models;
using PClinicPOS.Api.Services;
using Xunit;

namespace PClinicPOS.Tests;

public class PatientServiceTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static void SeedTenantsAndBranches(AppDbContext db)
    {
        var t1 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant1" };
        var t2 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant2" };
        db.Tenants.AddRange(t1, t2);
        db.Branches.AddRange(
            new Branch { Id = Guid.NewGuid(), TenantId = t1.Id, Name = "B1" },
            new Branch { Id = Guid.NewGuid(), TenantId = t2.Id, Name = "B2" }
        );
        db.SaveChanges();
    }

    [Fact]
    public async Task ListPatients_EnforcesTenantScoping_CannotReadOtherTenant()
    {
        using var db = CreateDb();
        SeedTenantsAndBranches(db);
        var tenant1Id = db.Tenants.First(t => t.Name == "Tenant1").Id;
        var tenant2Id = db.Tenants.First(t => t.Name == "Tenant2").Id;

        var patientInTenant2 = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Other",
            LastName = "Tenant",
            PhoneNumber = "+66999999999",
            TenantId = tenant2Id,
            CreatedAt = DateTime.UtcNow
        };
        db.Patients.Add(patientInTenant2);
        await db.SaveChangesAsync();

        var tenantContext = new StubTenantContext(tenant1Id);
        var service = new PatientService(db, tenantContext, new NoOpCacheService());

        var list = await service.ListAsync(tenant1Id, null);

        Assert.Empty(list);
        Assert.DoesNotContain(list, p => p.Id == patientInTenant2.Id);
    }

    [Fact]
    public async Task CreatePatient_DuplicatePhoneWithinTenant_ThrowsFriendlyError()
    {
        using var db = CreateDb();
        SeedTenantsAndBranches(db);
        var tenant1Id = db.Tenants.First(t => t.Name == "Tenant1").Id;
        var existing = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "User",
            PhoneNumber = "+66812345678",
            TenantId = tenant1Id,
            CreatedAt = DateTime.UtcNow
        };
        db.Patients.Add(existing);
        await db.SaveChangesAsync();

        var tenantContext = new StubTenantContext(tenant1Id);
        var service = new PatientService(db, tenantContext, new NoOpCacheService());
        var req = new CreatePatientRequest("New", "Patient", "+66812345678", tenant1Id, null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(req));
        Assert.Contains("phone number", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubTenantContext : ITenantContext
    {
        public StubTenantContext(Guid? tenantId) => TenantId = tenantId;
        public Guid? TenantId { get; }
        public Guid? UserId => null;
        public string? Role => "User";
        public IReadOnlyList<Guid> BranchIds => Array.Empty<Guid>();
    }
}
