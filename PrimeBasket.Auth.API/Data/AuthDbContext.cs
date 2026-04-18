using Microsoft.EntityFrameworkCore;
using PrimeBasket.Auth.API.Entities;

namespace PrimeBasket.Auth.API.Data;

public class AuthDbContext : DbContext
{
  public AuthDbContext(DbContextOptions<AuthDbContext> options)
      : base(options) { }

  public DbSet<User> Users => Set<User>();
}