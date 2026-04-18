using PrimeBasket.Auth.API.Data;
using PrimeBasket.Auth.API.DTOs;
using PrimeBasket.Auth.API.Entities;
using PrimeBasket.Auth.API.Interfaces.Auth;

namespace PrimeBasket.Auth.API.Services.Auth;

public class AuthService : IAuthService
{
  private readonly AuthDbContext _context;
  private readonly PasswordHasher _hasher;

  public AuthService(AuthDbContext context)
  {
    _context = context;
    _hasher = new PasswordHasher();
  }

  public async Task<string> RegisterAsync(RegisterRequest request)
  {
    // Check if user already exists
    var exists = _context.Users.Any(u => u.Email == request.Email);
    if (exists)
      return "User already exists";

    var user = new User
    {
      FullName = request.FullName,
      Email = request.Email,
      PasswordHash = _hasher.Hash(request.Password)
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return "User registered successfully";
  }
}