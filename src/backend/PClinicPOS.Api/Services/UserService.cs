using Microsoft.EntityFrameworkCore;
using PClinicPOS.Api.Auth;
using PClinicPOS.Api.Data;
using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public UserService(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId != default, ct);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var branchIds = user.UserBranches.Select(ub => ub.BranchId).ToList();
        var token = _jwt.GenerateToken(user, branchIds);
        return new LoginResult(token, new UserDto(user.Id, user.Email, user.Role, user.TenantId, branchIds));
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        var exists = await _db.Users.AnyAsync(u => u.TenantId == req.TenantId && u.Email == req.Email, ct);
        if (exists)
            throw new InvalidOperationException("A user with this email already exists in this tenant.");

        var role = NormalizeRole(req.Role);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = role,
            TenantId = req.TenantId
        };
        _db.Users.Add(user);

        foreach (var branchId in req.BranchIds ?? Array.Empty<Guid>())
            _db.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = branchId });

        await _db.SaveChangesAsync(ct);
        return new UserDto(user.Id, user.Email, user.Role, user.TenantId, req.BranchIds ?? Array.Empty<Guid>());
    }

    public async Task AssignRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user == null) throw new InvalidOperationException("User not found.");
        user.Role = NormalizeRole(role);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AssociateBranchesAsync(Guid userId, IReadOnlyList<Guid> branchIds, CancellationToken ct = default)
    {
        var existing = await _db.UserBranches.Where(ub => ub.UserId == userId).ToListAsync(ct);
        _db.UserBranches.RemoveRange(existing);
        foreach (var branchId in branchIds ?? Array.Empty<Guid>())
            _db.UserBranches.Add(new UserBranch { UserId = userId, BranchId = branchId });
        await _db.SaveChangesAsync(ct);
    }

    private static string NormalizeRole(string role) =>
        role?.Trim()?.ToLowerInvariant() switch
        {
            "admin" => "Admin",
            "user" => "User",
            "viewer" => "Viewer",
            _ => throw new ArgumentException("Role must be Admin, User, or Viewer.", nameof(role))
        };
}
