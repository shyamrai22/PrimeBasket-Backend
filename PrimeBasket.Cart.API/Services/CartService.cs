using Microsoft.EntityFrameworkCore;
using PrimeBasket.Cart.API.Data;
using PrimeBasket.Cart.API.DTOs;
using PrimeBasket.Cart.API.Entities;
using PrimeBasket.Cart.API.Interfaces;
using PrimeBasket.Cart.API.Exceptions;
using System.Net.Http.Json;

namespace PrimeBasket.Cart.API.Services;

public class CartService : ICartService
{
  private readonly CartDbContext _context;
  private readonly HttpClient _httpClient;

  public CartService(CartDbContext context, IHttpClientFactory httpClientFactory)
  {
    _context = context;
    _httpClient = httpClientFactory.CreateClient("ProductService");
  }

  public async Task<CartResponse> GetCartAsync(int userId)
  {
    var cart = await _context.Carts
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null)
    {
      return new CartResponse
      {
        UserId = userId
      };
    }

    return MapToResponse(cart);
  }

  public async Task<CartResponse> AddToCartAsync(int userId, AddToCartRequest request)
  {
    // Quantity validation
    if (request.Quantity <= 0 || request.Quantity > 100)
      throw new ArgumentException("Quantity must be between 1 and 100");

    // Get product stock (this replaces ProductExists)
    var stock = await GetProductStock(request.ProductId);

    if (stock == null)
      throw new NotFoundException("Product not found");

    if (request.Quantity > stock)
      throw new ArgumentException($"Only {stock} items available in stock");

    var cart = await _context.Carts
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null)
    {
      cart = new PrimeBasket.Cart.API.Entities.Cart { UserId = userId };
      _context.Carts.Add(cart);
    }

    var existingItem = cart.Items
        .FirstOrDefault(i => i.ProductId == request.ProductId);

    if (existingItem != null)
    {
      var newQuantity = existingItem.Quantity + request.Quantity;

      if (newQuantity > stock)
        throw new ArgumentException($"Only {stock} items available in stock");

      existingItem.Quantity = newQuantity;
    }
    else
    {
      cart.Items.Add(new CartItem
      {
        ProductId = request.ProductId,
        Quantity = request.Quantity
      });
    }

    await _context.SaveChangesAsync();

    return MapToResponse(cart);
  }

  // STOCK VALIDATION METHOD (core logic)
  private async Task<int?> GetProductStock(int productId)
  {
    var response = await _httpClient.GetAsync($"/api/products/{productId}/stock");

    Console.WriteLine($"Stock check → ID: {productId}, Status: {response.StatusCode}");

    if (!response.IsSuccessStatusCode)
      return null;

    var stock = await response.Content.ReadFromJsonAsync<int>();

    return stock;
  }

  private CartResponse MapToResponse(PrimeBasket.Cart.API.Entities.Cart cart)
  {
    return new CartResponse
    {
      Id = cart.Id,
      UserId = cart.UserId,
      Items = cart.Items.Select(i => new CartItemResponse
      {
        ProductId = i.ProductId,
        Quantity = i.Quantity
      }).ToList()
    };
  }

  public async Task ClearCartAsync(int userId)
  {
    var cart = await _context.Carts
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null)
      return;

    _context.CartItems.RemoveRange(cart.Items);
    _context.Carts.Remove(cart);

    await _context.SaveChangesAsync();
  }
}