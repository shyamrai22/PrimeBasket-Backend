using PrimeBasket.Auth.API.DTOs;

namespace PrimeBasket.Auth.API.Interfaces.Auth;

public interface IAuthService
{
  Task<string> RegisterAsync(RegisterRequest request);
}