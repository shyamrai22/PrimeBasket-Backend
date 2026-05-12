using Microsoft.EntityFrameworkCore;
using PrimeBasket.Orders.API.Entities;

namespace PrimeBasket.Orders.API.Data;

public class OrderDbContext : DbContext
{
  public OrderDbContext(DbContextOptions<OrderDbContext> options)
      : base(options) { }

  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // ---------------- ORDER ----------------
    modelBuilder.Entity<Order>()
        .Property(o => o.TotalAmount)
        .HasPrecision(18, 2);

    modelBuilder.Entity<Order>()
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