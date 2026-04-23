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
  private readonly IConfiguration _config;

  public AuthService(
      AuthDbContext context,
      TokenService tokenService,
      PasswordHasher hasher,
      IConfiguration config)
  {
    _context = context;
    _tokenService = tokenService;
    _hasher = hasher;
    _config = config;
  }

  public async Task<string> RegisterAsync(RegisterRequest request)
  {
    var email = request.Email.ToLower();

    var exists = await _context.Users
        .AnyAsync(u => u.Email.ToLower() == email);

    if (exists)
      return "User already exists";


    string role = "Customer";

    var adminKeyFromConfig = _config["AdminSettings:AdminKey"];

    if (!string.IsNullOrEmpty(request.AdminKey) &&
        request.AdminKey == adminKeyFromConfig)
    {
      role = "Admin";
    }

    var user = new User
    {
      FullName = request.FullName,
      Email = email,
      PasswordHash = _hasher.Hash(request.Password),
      Role = role
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

  public async Task<List<User>> GetAllUsersAsync()
  {
    return await _context.Users.ToListAsync();
  }
}