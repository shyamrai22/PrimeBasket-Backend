using Microsoft.EntityFrameworkCore;


namespace PrimeBasket.Product.API.Data;

public class ProductDbContext : DbContext
{
  public ProductDbContext(DbContextOptions<ProductDbContext> options)
      : base(options) { }

  public DbSet<PrimeBasket.Product.API.Entities.Product> Products => Set<PrimeBasket.Product.API.Entities.Product>();
}