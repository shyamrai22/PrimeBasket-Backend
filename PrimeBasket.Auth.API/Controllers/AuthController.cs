using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimeBasket.Auth.API.DTOs;
using PrimeBasket.Auth.API.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using PrimeBasket.Auth.API.Services.Auth;

namespace PrimeBasket.Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;
  private readonly PrimeBasket.Auth.API.Data.AuthDbContext _context;
  private readonly PasswordHasher _hasher;

  public AuthController(IAuthService authService, PrimeBasket.Auth.API.Data.AuthDbContext context, PasswordHasher hasher)
  {
    _authService = authService;
    _context = context;
    _hasher = hasher;
  }

  [AllowAnonymous]
  [HttpPost("seed-admin")]
  public async Task<IActionResult> SeedAdmin()
  {
      var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@primebasket.com");
      if (existing != null)
      {
          _context.Users.Remove(existing);
          await _context.SaveChangesAsync();
      }

      var adminUser = new PrimeBasket.Auth.API.Entities.User
      {
          FullName = "System Administrator",
          Email = "admin@primebasket.com",
          PasswordHash = _hasher.Hash("AdminPassword123!"),
          Role = "Admin",
          Status = "Approved"
      };

      _context.Users.Add(adminUser);
      await _context.SaveChangesAsync();

      return Ok(new { message = "Admin user 'admin@primebasket.com' reset with password 'AdminPassword123!'." });
  }

  [AllowAnonymous]
  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterRequest request)
  {
    var result = await _authService.RegisterAsync(request);

    if (result == "User already exists")
      return Conflict(new { message = "An account with this email already exists." });

    if (result.StartsWith("Unauthorized"))
      return Unauthorized(new { message = "Invalid merchant key. Please check your credentials." });

    return Ok(new { message = result });
  }

  [AllowAnonymous]
  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginRequest request)
  {
    var result = await _authService.LoginAsync(request);

    if (result == "Invalid credentials")
      return Unauthorized(new { message = "Invalid email or password." });

    // Check for account status blocks (Rejected)
    if (result.StartsWith("Your account"))
      return StatusCode(403, new { message = result });

    return Ok(new { token = result });
  }

  [Authorize]
  [HttpGet("secure")]
  public IActionResult SecureEndpoint()
  {
    return Ok("You are authenticated");
  }

  [Authorize(Roles = "Admin")]
  [HttpGet("users")]
  public async Task<IActionResult> GetUsers()
  {
    var users = await _authService.GetAllUsersAsync();
    return Ok(users);
  }

  [Authorize(Roles = "Admin")]
  [HttpPut("users/{id}/status")]
  public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
  {
    var result = await _authService.UpdateUserStatusAsync(id, status);
    if (!result) return NotFound("User not found");
    return Ok(new { message = "User status updated successfully" });
  }

  [Authorize]
  [HttpGet("status")]
  public async Task<IActionResult> GetStatus()
  {
    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
      return Unauthorized("User ID not found in token");

    var status = await _authService.GetUserStatusAsync(userId);
    return Ok(new { status = status });
  }
}