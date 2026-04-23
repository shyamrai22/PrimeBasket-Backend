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

  public async Task<PrimeBasket.Product.API.Entities.Product?> UpdateProductAsync(int id, ProductRequest request)
  {
    var product = await _context.Products.FindAsync(id);
    if (product == null) return null;

    product.Name = request.Name;
    product.Description = request.Description;
    product.Price = request.Price;
    product.Stock = request.Stock;

    await _context.SaveChangesAsync();
    return product;
  }

  public async Task UpdateStockAsync(PrimeBasket.Product.API.Entities.Product product)
  {
    _context.Products.Update(product);
    await _context.SaveChangesAsync();
  }

  public async Task<bool> DeleteProductAsync(int id)
  {
    var product = await _context.Products.FindAsync(id);
    if (product == null) return false;

    _context.Products.Remove(product);
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<PrimeBasket.Product.API.Entities.Product> GetByIdAsync(int id)
  {
    return await _context.Products.FindAsync(id);
  }
}