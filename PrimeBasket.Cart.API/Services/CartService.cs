using Microsoft.EntityFrameworkCore;
using PrimeBasket.Cart.API.Data;
using PrimeBasket.Cart.API.DTOs;
using PrimeBasket.Cart.API.Entities;
using PrimeBasket.Cart.API.Interfaces;

namespace PrimeBasket.Cart.API.Services;

public class CartService : ICartService
{
  private readonly CartDbContext _context;

  public CartService(CartDbContext context)
  {
    _context = context;
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
      existingItem.Quantity += request.Quantity;
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

  // Mapper (important)
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
}