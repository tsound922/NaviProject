using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviProject.Core.Services;

namespace NaviProject.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AuthController(UserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, message, token) = await userService.RegisterAsync(
            request.Username, request.Email, request.Password);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, message, token) = await userService.LoginAsync(
            request.Username, request.Password);

        if (!success)
            return Unauthorized(new { message });

        return Ok(new { message, token });
    }
}

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Username, string Password);