using PrimeBasket.Auth.API.DTOs;


namespace PrimeBasket.Auth.API.Interfaces.Auth;

public interface IAuthService
{
  Task<string> RegisterAsync(RegisterRequest request);
  Task<string> LoginAsync(LoginRequest request);
  Task<List<UserDto>> GetAllUsersAsync();
  Task<bool> UpdateUserStatusAsync(int userId, string status);
  Task<string> GetUserStatusAsync(int userId);
}