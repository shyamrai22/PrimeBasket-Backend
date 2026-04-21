using PrimeBasket.Cart.API.DTOs;

namespace PrimeBasket.Cart.API.Interfaces;

public interface ICartService
{
  Task<CartResponse> GetCartAsync(int userId);
  Task<CartResponse> AddToCartAsync(int userId, AddToCartRequest request);
}