using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PClinicPOS.Api.Models;
using PClinicPOS.Api.Services;

namespace PClinicPOS.Api.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly Auth.ITenantContext _tenantContext;

    public UsersController(IUserService userService, Auth.ITenantContext tenantContext)
    {
        _userService = userService;
        _tenantContext = tenantContext;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId ?? dto.TenantId;
        if (tenantId == default)
            return BadRequest(new ErrorResponse { Error = "TenantId is required." });

        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new ErrorResponse { Error = "Email and Password are required." });

        var req = new CreateUserRequest(dto.Email, dto.Password, dto.Role ?? "User", tenantId, dto.BranchIds ?? new List<Guid>());
        try
        {
            var user = await _userService.CreateUserAsync(req, ct);
            return CreatedAtAction(nameof(Create), user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPut("{userId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleDto dto, CancellationToken ct)
    {
        try
        {
            await _userService.AssignRoleAsync(userId, dto.Role, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPut("{userId:guid}/branches")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssociateBranches(Guid userId, [FromBody] AssociateBranchesDto dto, CancellationToken ct)
    {
        await _userService.AssociateBranchesAsync(userId, dto.BranchIds ?? new List<Guid>(), ct);
        return NoContent();
    }
}

public record CreateUserDto(string Email, string Password, string? Role, Guid TenantId, IReadOnlyList<Guid>? BranchIds);
public record AssignRoleDto(string Role);
public record AssociateBranchesDto(IReadOnlyList<Guid>? BranchIds);
