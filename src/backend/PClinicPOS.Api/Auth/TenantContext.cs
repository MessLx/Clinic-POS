using System.Security.Claims;

namespace PClinicPOS.Api.Auth;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _http;

    public TenantContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid? TenantId => GetClaimGuid("tenant_id");
    public Guid? UserId => GetClaimGuid(ClaimTypes.NameIdentifier);
    public string? Role => _http.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public IReadOnlyList<Guid> BranchIds
    {
        get
        {
            var branchIds = _http.HttpContext?.User?.FindFirst("branch_ids")?.Value;
            if (string.IsNullOrEmpty(branchIds)) return Array.Empty<Guid>();
            return branchIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Guid.TryParse(x, out var g) ? g : Guid.Empty)
                .Where(x => x != Guid.Empty)
                .ToList();
        }
    }

    private Guid? GetClaimGuid(string type)
    {
        var value = _http.HttpContext?.User?.FindFirst(type)?.Value;
        return Guid.TryParse(value, out var g) ? g : null;
    }
}
