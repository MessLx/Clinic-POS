using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Auth;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, IReadOnlyList<Guid> branchIds)
    {
        var key = _config["Jwt:Key"] ?? "dev-secret-key-min-32-chars-long!!";
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("tenant_id", user.TenantId.ToString())
        };
        if (branchIds.Count > 0)
            claims.Add(new Claim("branch_ids", string.Join(",", branchIds.Select(x => x.ToString()))));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
