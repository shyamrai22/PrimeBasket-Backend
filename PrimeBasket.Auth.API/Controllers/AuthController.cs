using Microsoft.AspNetCore.Mvc;
using PrimeBasket.Auth.API.DTOs;
using PrimeBasket.Auth.API.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;

namespace PrimeBasket.Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;

  public AuthController(IAuthService authService)
  {
    _authService = authService;
  }

  [AllowAnonymous]
  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterRequest request)
  {
    var result = await _authService.RegisterAsync(request);

    if (result == "User already exists")
      return BadRequest(result);

    return Ok(result);
  }

  [AllowAnonymous]
  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginRequest request)
  {
    var result = await _authService.LoginAsync(request);

    if (result == "Invalid credentials")
      return Unauthorized(result);

    return Ok(result);
  }

  [Authorize]
  [HttpGet("secure")]
  public IActionResult SecureEndpoint()
  {
    return Ok("You are authenticated");
  }
}