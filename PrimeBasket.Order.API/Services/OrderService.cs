using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PrimeBasket.Orders.API.Data;
using PrimeBasket.Orders.API.DTOs;
using PrimeBasket.Orders.API.Entities;
using PrimeBasket.Orders.API.Exceptions;
using PrimeBasket.Orders.API.Interfaces;
using PrimeBasket.Orders.API.Enums;

namespace PrimeBasket.Orders.API.Services;

public class OrderService : IOrderService
{
  private readonly OrderDbContext _context;
  private readonly HttpClient _productClient;
  private readonly HttpClient _cartClient;
  private readonly HttpClient _paymentClient;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public OrderService(
      OrderDbContext context,
      IHttpClientFactory httpClientFactory,
      IHttpContextAccessor httpContextAccessor)
  {
    _context = context;
    _productClient = httpClientFactory.CreateClient("ProductService");
    _cartClient = httpClientFactory.CreateClient("CartService");
    _paymentClient = httpClientFactory.CreateClient("PaymentService");
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<OrderResponse> CheckoutAsync(int userId, CheckoutRequest requestDto)
  {
    var token = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString();

    var request = new HttpRequestMessage(HttpMethod.Get, "/api/cart");
    request.Headers.Add("Authorization", token);

    var response = await _cartClient.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
      var errorBody = await response.Content.ReadAsStringAsync();
      throw new BadRequestException($"Failed to fetch cart: {errorBody}");
    }

    var cart = await response.Content.ReadFromJsonAsync<CartResponse>();

    if (cart?.Items == null || !cart.Items.Any())
      throw new BadRequestException("Cart is empty");

    // Validate stock
    foreach (var item in cart.Items)
    {
      var stock = await GetStock(item.ProductId);
      if (stock == null || item.Quantity > stock)
        throw new BadRequestException($"Insufficient stock for product {item.ProductId}");
    }

    var orderItems = await PrepareOrderItems(cart.Items);
    var amount = orderItems.Sum(i => i.Price * i.Quantity);

    var paymentResult = await ProcessPayment(userId, amount, requestDto.PaymentMethod);

    if (!paymentResult.Status.Equals("Success", StringComparison.OrdinalIgnoreCase) && 
        !paymentResult.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
      throw new BadRequestException(paymentResult.Message ?? "Payment failed");

    foreach (var item in cart.Items)
      await ReduceStock(item.ProductId, item.Quantity);

    var order = new Order
    {
      UserId = userId,
      PaymentId = paymentResult.PaymentId,
      PaymentMethod = requestDto.PaymentMethod,
      Status = paymentResult.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ? OrderStatus.Pending : OrderStatus.Paid,
      TotalAmount = amount,
      Items = orderItems
    };

    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    await ClearCart(token);

    return MapToResponse(order);
  }

  public async Task<OrderResponse> BuyNowAsync(int userId, BuyNowRequest request)
  {
    var stock = await GetStock(request.ProductId);

    if (stock == null || request.Quantity > stock)
      throw new BadRequestException("Insufficient stock");

    var product = await GetProduct(request.ProductId);

    if (product == null)
      throw new BadRequestException("Product not found");

    var amount = product.Price * request.Quantity;

    var paymentResult = await ProcessPayment(userId, amount, request.PaymentMethod);

    if (!paymentResult.Status.Equals("Success", StringComparison.OrdinalIgnoreCase) && 
        !paymentResult.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
      throw new BadRequestException(paymentResult.Message ?? "Payment failed");

    await ReduceStock(request.ProductId, request.Quantity);

    var order = new Order
    {
      UserId = userId,
      PaymentId = paymentResult.PaymentId,
      PaymentMethod = request.PaymentMethod,
      Status = paymentResult.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ? OrderStatus.Pending : OrderStatus.Paid,
      TotalAmount = amount,
      Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Price = product.Price
                }
            }
    };

    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    return MapToResponse(order);
  }

  private async Task<PaymentResult> ProcessPayment(int userId, decimal amount, string paymentMethod)
  {
    var token = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(token)) throw new Exception("Authorization token is missing");

    var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/pay");
    request.Headers.Add("Authorization", token);

    request.Content = JsonContent.Create(new
    {
      orderId = new Random().Next(100000, 999999),
      amount,
      paymentMethod,
      idempotencyKey = Guid.NewGuid().ToString()
    });

    var response = await _paymentClient.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
      var errorBody = await response.Content.ReadAsStringAsync();
      throw new Exception($"Payment API failed: {errorBody}");
    }

    return await response.Content.ReadFromJsonAsync<PaymentResult>()
           ?? throw new Exception("Invalid payment response");
  }

  private OrderResponse MapToResponse(Order order)
  {
    return new OrderResponse
    {
      OrderId = order.Id,
      UserId = order.UserId,
      CreatedAt = order.CreatedAt,
      Status = order.Status.ToString(),
      TotalAmount = order.TotalAmount,
      PaymentId = order.PaymentId,
      PaymentMethod = order.PaymentMethod,
      Items = order.Items.Select(i => new OrderItemResponse
      {
        OrderItemId = i.Id,
        ProductId = i.ProductId,
        Quantity = i.Quantity,
        Price = i.Price,
        TotalPrice = i.TotalPrice
      }).ToList()
    };
  }

  public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
  {
    var orders = await _context.Orders
        .Include(o => o.Items)
        .Where(o => o.UserId == userId)
        .ToListAsync();

    return orders.Select(MapToResponse).ToList();
  }

  public async Task<string> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
  {
    var order = await _context.Orders.FindAsync(orderId);

    if (order == null)
      throw new NotFoundException("Order not found");

    if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
      throw new BadRequestException("Invalid status");

    order.Status = newStatus;
    await _context.SaveChangesAsync();

    return $"Order status updated to {request.Status}";
  }

  public async Task<AdminStatsResponse> GetAdminStatsAsync()
  {
    var orders = await _context.Orders.ToListAsync();

    return new AdminStatsResponse
    {
      TotalRevenue = orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
      TotalOrders = orders.Count,
      PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
      CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid),
      CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled)
    };
  }

  public async Task<List<OrderResponse>> GetAllOrdersAsync()
  {
    var orders = await _context.Orders
        .Include(o => o.Items)
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();

    return orders.Select(o => new OrderResponse
    {
      OrderId = o.Id,
      UserId = o.UserId,
      CreatedAt = o.CreatedAt,
      TotalAmount = o.TotalAmount,
      Status = o.Status.ToString(),
      PaymentMethod = o.PaymentMethod,
      Items = o.Items.Select(oi => new OrderItemResponse
      {
        ProductId = oi.ProductId,
        Quantity = oi.Quantity,
        Price = oi.Price
      }).ToList()
    }).ToList();
  }

  public async Task<string> CancelOrderAsync(int userId, int orderId)
  {
    var order = await _context.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

    if (order == null)
      throw new NotFoundException("Order not found or you don't have permission to cancel it.");

    if (order.Status == OrderStatus.Cancelled)
      throw new BadRequestException("Order is already cancelled.");

    if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
      throw new BadRequestException($"Cannot cancel order that has been {order.Status.ToString().ToLower()}.");

    // 1. Restore Stock
    foreach (var item in order.Items)
    {
      await AddStock(item.ProductId, item.Quantity);
    }

    // 2. Refund if Paid via Wallet
    if (order.Status == OrderStatus.Paid && order.PaymentMethod == "Wallet")
    {
      await RefundWallet(userId, order.TotalAmount);
    }

    // 3. Update Status
    order.Status = OrderStatus.Cancelled;
    await _context.SaveChangesAsync();

    return "Order cancelled successfully. Stock restored and funds refunded (if applicable).";
  }

  private async Task AddStock(int productId, int quantity)
  {
    var token = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString();
    var request = new HttpRequestMessage(HttpMethod.Post, $"/api/products/{productId}/add-stock");
    request.Headers.Add("Authorization", token);
    request.Content = JsonContent.Create(new { quantity });

    var response = await _productClient.SendAsync(request);
    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync();
      throw new Exception($"Failed to restore stock: {error}");
    }
  }

  private async Task RefundWallet(int userId, decimal amount)
  {
    var token = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString();
    var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/wallet/add-money");
    request.Headers.Add("Authorization", token);
    request.Content = JsonContent.Create(new { amount });

    var response = await _paymentClient.SendAsync(request);
    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync();
      throw new Exception($"Failed to refund wallet: {error}");
    }
  }

  private async Task<int?> GetStock(int productId)
  {
    var response = await _productClient.GetAsync($"/api/products/{productId}/stock");
    if (!response.IsSuccessStatusCode) return null;

    var stock = await response.Content.ReadFromJsonAsync<int>();
    return stock;
  }

  private async Task<ProductResponse?> GetProduct(int productId)
  {
    var response = await _productClient.GetAsync($"/api/products/{productId}");
    if (!response.IsSuccessStatusCode) return null;

    return await response.Content.ReadFromJsonAsync<ProductResponse>();
  }

  private async Task ReduceStock(int productId, int quantity)
  {
    var token = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(token)) throw new Exception("Authorization token is missing");

    var request = new HttpRequestMessage(HttpMethod.Post, $"/api/products/{productId}/reduce-stock");
    request.Headers.Add("Authorization", token);
    request.Content = JsonContent.Create(new { quantity });

    var response = await _productClient.SendAsync(request);
    if (!response.IsSuccessStatusCode)
    {
      var errorBody = await response.Content.ReadAsStringAsync();
      throw new Exception($"Failed to reduce stock for product {productId}: {errorBody}");
    }
  }

  private async Task<List<OrderItem>> PrepareOrderItems(IEnumerable<CartItemResponse> cartItems)
  {
    var orderItems = new List<OrderItem>();

    foreach (var item in cartItems)
    {
      var product = await GetProduct(item.ProductId);
      if (product == null) throw new Exception($"Product {item.ProductId} no longer exists");

      orderItems.Add(new OrderItem
      {
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        Price = product.Price
      });
    }

    return orderItems;
  }

  private async Task ClearCart(string token)
  {
    var request = new HttpRequestMessage(HttpMethod.Delete, "/api/cart");
    request.Headers.Add("Authorization", token);

    var response = await _cartClient.SendAsync(request);
    if (!response.IsSuccessStatusCode)
    {
      var errorBody = await response.Content.ReadAsStringAsync();
      throw new Exception($"Failed to clear cart after order: {errorBody}");
    }
  }
}