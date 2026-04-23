using Microsoft.EntityFrameworkCore;
using PrimeBasket.Orders.API.Entities;

namespace PrimeBasket.Orders.API.Data;

public class OrderDbContext : DbContext
{
  public OrderDbContext(DbContextOptions<OrderDbContext> options)
      : base(options) { }

  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}