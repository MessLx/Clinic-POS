using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Services;

public interface IUserService
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest req, CancellationToken ct = default);
    Task AssignRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task AssociateBranchesAsync(Guid userId, IReadOnlyList<Guid> branchIds, CancellationToken ct = default);
}

public record LoginResult(string Token, UserDto User);

public record UserDto(Guid Id, string Email, string Role, Guid TenantId, IReadOnlyList<Guid> BranchIds);

public record CreateUserRequest(string Email, string Password, string Role, Guid TenantId, IReadOnlyList<Guid> BranchIds);
