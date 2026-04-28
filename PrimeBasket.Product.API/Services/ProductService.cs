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
      Stock = request.Stock,
      ImageUrl = request.ImageUrl
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
    product.ImageUrl = request.ImageUrl;

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

  public async Task<int> SeedProductsAsync()
  {
    // Clear existing products to ensure a clean slate
    var existingProducts = await _context.Products.ToListAsync();
    if (existingProducts.Any())
    {
      _context.Products.RemoveRange(existingProducts);
      await _context.SaveChangesAsync();
    }

    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("https://fakestoreapi.com/products");
    
    if (!response.IsSuccessStatusCode)
    {
      throw new Exception("Failed to fetch dummy products from FakeStoreAPI");
    }

    var fakeProducts = await response.Content.ReadFromJsonAsync<List<FakeStoreProduct>>();
    
    if (fakeProducts == null || !fakeProducts.Any())
    {
      return 0;
    }

    var productsToAdd = fakeProducts.Select(fp => new PrimeBasket.Product.API.Entities.Product
    {
      Name = fp.Title,
      Description = fp.Description,
      Price = fp.Price,
      Stock = new Random().Next(10, 100), // Random stock between 10 and 100
      ImageUrl = fp.Image
    }).ToList();

    await _context.Products.AddRangeAsync(productsToAdd);
    await _context.SaveChangesAsync();

    return productsToAdd.Count;
  }
}