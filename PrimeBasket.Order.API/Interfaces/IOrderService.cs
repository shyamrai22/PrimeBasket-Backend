using PrimeBasket.Orders.API.DTOs;

namespace PrimeBasket.Orders.API.Interfaces;

public interface IOrderService
{
  Task<OrderResponse> CheckoutAsync(int userId, CheckoutRequest request);

  Task<OrderResponse> BuyNowAsync(int userId, BuyNowRequest request);

  Task<List<OrderResponse>> GetUserOrdersAsync(int userId);

  Task<string> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);
  Task<AdminStatsResponse> GetAdminStatsAsync();
  Task<List<OrderResponse>> GetAllOrdersAsync();
  Task<string> CancelOrderAsync(int userId, int orderId);
}