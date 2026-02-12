using Microsoft.EntityFrameworkCore;
using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Data;

public static class DataSeeder
{
    public const string DefaultPassword = "Password1!";
    public static readonly Guid SeedTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Branch1Id = Guid.Parse("22222222-2222-2222-2222-222222222221");
    public static readonly Guid Branch2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminUserId = Guid.Parse("33333333-3333-3333-3333-333333333331");
    public static readonly Guid UserUserId = Guid.Parse("33333333-3333-3333-3333-333333333332");
    public static readonly Guid ViewerUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Tenants.AnyAsync())
            return;

        var tenant = new Tenant { Id = SeedTenantId, Name = "Pea Aura Wellness" };
        db.Tenants.Add(tenant);
        db.Branches.AddRange(
            new Branch { Id = Branch1Id, TenantId = SeedTenantId, Name = "Bangkok Branch" },
            new Branch { Id = Branch2Id, TenantId = SeedTenantId, Name = "Chiang Mai Branch" }
        );

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);
        var admin = new User { Id = AdminUserId, Email = "admin@peaaura.local", PasswordHash = passwordHash, Role = "Admin", TenantId = SeedTenantId };
        var user = new User { Id = UserUserId, Email = "user@peaaura.local", PasswordHash = passwordHash, Role = "User", TenantId = SeedTenantId };
        var viewer = new User { Id = ViewerUserId, Email = "viewer@peaaura.local", PasswordHash = passwordHash, Role = "Viewer", TenantId = SeedTenantId };
        db.Users.AddRange(admin, user, viewer);
        db.UserBranches.AddRange(
            new UserBranch { UserId = AdminUserId, BranchId = Branch1Id },
            new UserBranch { UserId = AdminUserId, BranchId = Branch2Id },
            new UserBranch { UserId = UserUserId, BranchId = Branch1Id },
            new UserBranch { UserId = ViewerUserId, BranchId = Branch1Id }
        );

        await db.SaveChangesAsync();
    }
}
