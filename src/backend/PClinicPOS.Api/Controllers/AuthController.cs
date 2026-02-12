using Microsoft.AspNetCore.Mvc;
using PClinicPOS.Api.Models;
using PClinicPOS.Api.Services;

namespace PClinicPOS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _userService.LoginAsync(req.Email, req.Password, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ErrorResponse { Error = "Invalid email or password." });
        }
    }
}

public record LoginRequest(string Email, string Password);
