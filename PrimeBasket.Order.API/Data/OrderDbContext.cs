using Microsoft.EntityFrameworkCore;
using PrimeBasket.Orders.API.Entities;
using Order = PrimeBasket.Orders.API.Entities.Order;

namespace PrimeBasket.Orders.API.Data;

public class OrderDbContext : DbContext
{
  public OrderDbContext(DbContextOptions<OrderDbContext> options)
      : base(options) { }

  public DbSet<PrimeBasket.Orders.API.Entities.Order> Orders => Set<PrimeBasket.Orders.API.Entities.Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // ---------------- ORDER ----------------
    modelBuilder.Entity<PrimeBasket.Orders.API.Entities.Order>()
        .Property(o => o.TotalAmount)
        .HasPrecision(18, 2);

    modelBuilder.Entity<PrimeBasket.Orders.API.Entities.Order>()
        .HasIndex(o => o.UserId);

    // ---------------- ORDER ITEM ----------------
    modelBuilder.Entity<OrderItem>()
        .Property(i => i.Price)
        .HasPrecision(18, 2);

    modelBuilder.Entity<OrderItem>()
        .HasOne(i => i.Order)
        .WithMany(o => o.Items)
        .HasForeignKey(i => i.OrderId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}

