using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Auth;

public interface IJwtService
{
    string GenerateToken(User user, IReadOnlyList<Guid> branchIds);
}
