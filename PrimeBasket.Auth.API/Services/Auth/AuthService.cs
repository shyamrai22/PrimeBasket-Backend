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
    var merchantKeyFromConfig = _config["RoleSettings:MerchantKey"];

    if (request.Role == "Admin" && !string.IsNullOrEmpty(request.RoleKey) && request.RoleKey == adminKeyFromConfig)
    {
      role = "Admin";
    }
    else if (request.Role == "Merchant")
    {
      // If merchant key is missing or incorrect, fail explicitly
      if (string.IsNullOrEmpty(request.RoleKey) || request.RoleKey != merchantKeyFromConfig)
        return "Unauthorized: Invalid or missing merchant key.";

      role = "Merchant";
    }

    var user = new User
    {
      FullName = request.FullName,
      Email = email,
      PasswordHash = _hasher.Hash(request.Password),
      Role = role,
      Status = (role == "Merchant") ? "Pending" : "Approved",
      BusinessName = request.BusinessName,
      BusinessType = request.BusinessType,
      StoreDescription = request.StoreDescription
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

    if (user.Status == "Rejected")
      return "Your account has been rejected. Please contact support.";

    return _tokenService.GenerateToken(user);
  }

  public async Task<List<UserDto>> GetAllUsersAsync()
  {
    return await _context.Users.Select(u => new UserDto
    {
      Id = u.Id,
      FullName = u.FullName,
      Email = u.Email,
      Role = u.Role,
      Status = u.Status,
      BusinessName = u.BusinessName,
      BusinessType = u.BusinessType,
      StoreDescription = u.StoreDescription,
      CreatedAt = u.CreatedAt
    }).ToListAsync();
  }

  public async Task<bool> UpdateUserStatusAsync(int userId, string status)
  {
    var user = await _context.Users.FindAsync(userId);
    if (user == null) return false;

    user.Status = status;
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<string> GetUserStatusAsync(int userId)
  {
    var user = await _context.Users.FindAsync(userId);
    return user?.Status ?? "";
  }
}