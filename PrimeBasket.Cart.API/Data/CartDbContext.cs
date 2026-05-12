using Microsoft.EntityFrameworkCore;

namespace PrimeBasket.Cart.API.Data;

public class CartDbContext : DbContext
{
  public CartDbContext(DbContextOptions<CartDbContext> options)
      : base(options) { }

  public DbSet<PrimeBasket.Cart.API.Entities.Cart> Carts
      => Set<PrimeBasket.Cart.API.Entities.Cart>();

  public DbSet<PrimeBasket.Cart.API.Entities.CartItem> CartItems
      => Set<PrimeBasket.Cart.API.Entities.CartItem>();
}