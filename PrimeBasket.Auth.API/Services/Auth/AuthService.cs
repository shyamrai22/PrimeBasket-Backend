using PrimeBasket.Auth.API.Data;
using PrimeBasket.Auth.API.DTOs;
using PrimeBasket.Auth.API.Entities;
using PrimeBasket.Auth.API.Interfaces.Auth;
using Microsoft.EntityFrameworkCore;

namespace PrimeBasket.Auth.API.Services.Auth;

public class AuthService : IAuthService
{
  private readonly AuthDbContext _context;
  private readonly PasswordHasher _hasher;
  private readonly TokenService _tokenService;

  public AuthService(AuthDbContext context, TokenService tokenService, PasswordHasher hasher)
  {
    _context = context;
    _tokenService = tokenService;
    _hasher = hasher;
  }

  public async Task<string> RegisterAsync(RegisterRequest request)
  {
    var email = request.Email.ToLower();

    var exists = await _context.Users
        .AnyAsync(u => u.Email.ToLower() == email);

    if (exists)
      return "User already exists";

    var user = new User
    {
      FullName = request.FullName,
      Email = email,
      PasswordHash = _hasher.Hash(request.Password)
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return "User registered successfully";
  }

  public async Task<string> LoginAsync(LoginRequest request)
  {
    var email = request.Email.ToLower();

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

    if (user == null)
      return "Invalid credentials";

    var isValid = _hasher.Verify(request.Password, user.PasswordHash);

    if (!isValid)
      return "Invalid credentials";

    return _tokenService.GenerateToken(user);
  }
}