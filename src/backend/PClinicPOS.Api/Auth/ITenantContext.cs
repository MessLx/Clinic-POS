namespace PClinicPOS.Api.Auth;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    string? Role { get; }
    IReadOnlyList<Guid> BranchIds { get; }
}
