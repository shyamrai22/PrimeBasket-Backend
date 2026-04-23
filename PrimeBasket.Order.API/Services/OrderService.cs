using PrimeBasket.Orders.API.Interfaces;
using PrimeBasket.Orders.API.DTOs;
using PrimeBasket.Orders.API.Entities;
using PrimeBasket.Orders.API.Data;
using PrimeBasket.Orders.API.Exceptions;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PrimeBasket.Orders.API.Services;

public class OrderService : IOrderService
{
  private readonly OrderDbContext _context;
  private readonly HttpClient _productClient;
  private readonly HttpClient _cartClient;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public OrderService(
      OrderDbContext context,
      IHttpClientFactory httpClientFactory,
      IHttpContextAccessor httpContextAccessor)
  {
    _context = context;
    _productClient = httpClientFactory.CreateClient("ProductService");
    _cartClient = httpClientFactory.CreateClient("CartService");
    _httpContextAccessor = httpClientFactory == null ? throw new ArgumentNullException(nameof(httpClientFactory)) : httpContextAccessor;
  }

  public async Task<OrderResponse> CheckoutAsync(int userId)
  {
    var token = _httpContextAccessor.HttpContext!
        .Request.Headers["Authorization"]
        .ToString();

    // -------------------- GET CART --------------------
    var request = new HttpRequestMessage(HttpMethod.Get, "/api/cart");
    request.Headers.Add("Authorization", token);

    var response = await _cartClient.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync();
      throw new BadRequestException($"Failed to fetch cart: {error}");
    }

    var cart = await response.Content.ReadFromJsonAsync<CartResponse>();

    if (cart?.Items == null || !cart.Items.Any())
      throw new BadRequestException("Cart is empty");

    // -------------------- VALIDATE STOCK --------------------
    foreach (var item in cart.Items)
    {
      var stock = await GetStock(item.ProductId);

      if (stock == null)
        throw new BadRequestException($"Product {item.ProductId} not found");

      if (item.Quantity > stock)
        throw new BadRequestException(
            $"Only {stock} available for product {item.ProductId}");
    }

    var deductedItems = new List<(int productId, int quantity)>();
    OrderResponse result;

    try
    {
      // -------------------- REDUCE STOCK --------------------
      foreach (var item in cart.Items)
      {
        await ReduceStock(item.ProductId, item.Quantity);
        deductedItems.Add((item.ProductId, item.Quantity));
      }

      // -------------------- CREATE ORDER --------------------
      var order = new Order
      {
        UserId = userId,
        Items = cart.Items.Select(i => new OrderItem
        {
          ProductId = i.ProductId,
          Quantity = i.Quantity
        }).ToList()
      };

      _context.Orders.Add(order);
      await _context.SaveChangesAsync();

      result = new OrderResponse
      {
        OrderId = order.Id,
        UserId = order.UserId,
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(i => new OrderItemResponse
        {
          ProductId = i.ProductId,
          Quantity = i.Quantity
        }).ToList()
      };
    }
    catch (Exception ex)
    {
      // -------------------- ROLLBACK STOCK --------------------
      foreach (var item in deductedItems)
      {
        await RestoreStock(item.productId, item.quantity);
      }

      throw new Exception($"Order failed and rolled back: {ex.Message}");
    }

    // -------------------- CLEAR CART (NON-CRITICAL) --------------------
    try
    {
      await ClearCart(token);
    }
    catch
    {
      // Optional: log this instead of throwing
    }

    return result;
  }

  public async Task<OrderResponse> BuyNowAsync(int userId, BuyNowRequest request)
  {
    var token = _httpContextAccessor.HttpContext!
        .Request.Headers["Authorization"]
        .ToString();

    // ---------------- VALIDATION ----------------
    if (request.Quantity <= 0)
      throw new BadRequestException("Quantity must be greater than 0");

    var stock = await GetStock(request.ProductId);

    if (stock == null)
      throw new BadRequestException($"Product {request.ProductId} not found");

    if (request.Quantity > stock)
      throw new BadRequestException(
          $"Only {stock} available for product {request.ProductId}");

    var deductedItems = new List<(int productId, int quantity)>();
    OrderResponse result;

    try
    {
      // ---------------- REDUCE STOCK ----------------
      await ReduceStock(request.ProductId, request.Quantity);
      deductedItems.Add((request.ProductId, request.Quantity));

      // ---------------- CREATE ORDER ----------------
      var order = new Order
      {
        UserId = userId,
        Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                }
            }
      };

      _context.Orders.Add(order);
      await _context.SaveChangesAsync();

      result = new OrderResponse
      {
        OrderId = order.Id,
        UserId = order.UserId,
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(i => new OrderItemResponse
        {
          ProductId = i.ProductId,
          Quantity = i.Quantity
        }).ToList()
      };
    }
    catch (Exception ex)
    {
      // ---------------- ROLLBACK ----------------
      foreach (var item in deductedItems)
      {
        await RestoreStock(item.productId, item.quantity);
      }

      throw new Exception($"BuyNow failed and rolled back: {ex.Message}");
    }

    return result;
  }

  public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
  {
    var orders = await _context.Orders
        .Where(o => o.UserId == userId)
        .Select(o => new OrderResponse
        {
          OrderId = o.Id,
          UserId = o.UserId,
          CreatedAt = o.CreatedAt,
          Items = o.Items.Select(i => new OrderItemResponse
          {
            ProductId = i.ProductId,
            Quantity = i.Quantity
          }).ToList()
        })
        .ToListAsync();

    return orders;
  }

  private async Task RestoreStock(int productId, int quantity)
  {
    var token = _httpContextAccessor.HttpContext!
        .Request.Headers["Authorization"]
        .ToString();

    var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"/api/products/{productId}/add-stock");

    request.Headers.Add("Authorization", token);
    request.Content = JsonContent.Create(new { quantity });

    await _productClient.SendAsync(request);
  }

  private async Task<int?> GetStock(int productId)
  {
    var response = await _productClient.GetAsync($"/api/products/{productId}/stock");

    if (!response.IsSuccessStatusCode)
      return null;

    return await response.Content.ReadFromJsonAsync<int>();
  }

  private async Task ReduceStock(int productId, int quantity)
  {
    var token = _httpContextAccessor.HttpContext!
        .Request.Headers["Authorization"]
        .ToString();

    var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"/api/products/{productId}/reduce-stock");

    request.Headers.Add("Authorization", token);
    request.Content = JsonContent.Create(new { quantity });

    var response = await _productClient.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync();
      throw new Exception($"Failed to reduce stock: {error}");
    }
  }

  private async Task ClearCart(string token)
  {
    var request = new HttpRequestMessage(HttpMethod.Delete, "/api/cart");
    request.Headers.Add("Authorization", token);

    var response = await _cartClient.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync();
      throw new Exception($"Failed to clear cart: {error}");
    }
  }
}