using PrimeBasket.Orders.API.DTOs;

namespace PrimeBasket.Orders.API.Interfaces;

public interface IOrderService
{
  //Task<string> CreateOrderAsync(int userId, CreateOrderRequest request);
  Task<OrderResponse> CheckoutAsync(int userId);
  Task<OrderResponse> BuyNowAsync(int userId, BuyNowRequest request);
  Task<List<OrderResponse>> GetUserOrdersAsync(int userId);
}