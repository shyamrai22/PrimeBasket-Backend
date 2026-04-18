using Microsoft.EntityFrameworkCore;
using PrimeBasket.Product.API.Data;
using PrimeBasket.Product.API.DTOs;
using PrimeBasket.Product.API.Interfaces;

namespace PrimeBasket.Product.API.Services;

public class ProductService : IProductService
{
  private readonly ProductDbContext _context;

  public ProductService(ProductDbContext context)
  {
    _context = context;
  }

  public async Task<PrimeBasket.Product.API.Entities.Product> AddProductAsync(ProductRequest request)
  {
    var product = new PrimeBasket.Product.API.Entities.Product
    {
      Name = request.Name,
      Description = request.Description,
      Price = request.Price,
      Stock = request.Stock
    };

    _context.Products.Add(product);
    await _context.SaveChangesAsync();

    return product;
  }

  public async Task<List<PrimeBasket.Product.API.Entities.Product>> GetAllAsync()
  {
    return await _context.Products.ToListAsync();
  }
}