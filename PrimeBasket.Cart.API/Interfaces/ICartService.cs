using PrimeBasket.Cart.API.DTOs;

namespace PrimeBasket.Cart.API.Interfaces;

public interface ICartService
{
  Task<CartResponse> GetCartAsync(int userId);
  Task<CartResponse> AddToCartAsync(int userId, AddToCartRequest request);
  Task<CartResponse> UpdateQuantityAsync(int userId, int productId, int quantity);
  Task<CartResponse> RemoveItemAsync(int userId, int productId);
  Task ClearCartAsync(int userId);
}