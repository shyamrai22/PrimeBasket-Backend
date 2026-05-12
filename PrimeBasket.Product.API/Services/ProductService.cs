using Microsoft.EntityFrameworkCore;
using PrimeBasket.Product.API.Data;
using PrimeBasket.Product.API.DTOs;
using PrimeBasket.Product.API.Interfaces;
using PrimeBasket.Product.API.Entities;

namespace PrimeBasket.Product.API.Services;

public class ProductService : IProductService
{
  private readonly ProductDbContext _context;

  public ProductService(ProductDbContext context)
  {
    _context = context;
  }

  public async Task<PrimeBasket.Product.API.Entities.Product> AddProductAsync(ProductRequest request, int merchantId)
  {
    var product = new PrimeBasket.Product.API.Entities.Product
    {
      Name = request.Name,
      Description = request.Description,
      Price = request.Price,
      Stock = request.Stock,
      ImageUrl = request.ImageUrl,
      Category = request.Category,
      MerchantId = merchantId,
      Status = "Active" // Default to Active for now, can be changed based on business rules
    };

    _context.Products.Add(product);
    await _context.SaveChangesAsync();

    return product;
  }

  public async Task<List<PrimeBasket.Product.API.Entities.Product>> GetAllAsync()
  {
    // Return all products for platform-wide listings
    return await _context.Products.ToListAsync();
  }

  public async Task<List<PrimeBasket.Product.API.Entities.Product>> GetByMerchantIdAsync(int merchantId)
  {
    return await _context.Products
        .Where(p => p.MerchantId == merchantId)
        .ToListAsync();
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
    product.Category = request.Category;

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

  public async Task<PrimeBasket.Product.API.Entities.Product?> GetByIdAsync(int id)
  {
    return await _context.Products.FindAsync(id);
  }

  public async Task<bool> UpdateStatusAsync(int productId, string status)
  {
    var product = await _context.Products.FindAsync(productId);
    if (product == null) return false;

    product.Status = status;
    await _context.SaveChangesAsync();
    return true;
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
      Stock = new Random().Next(10, 100),
      ImageUrl = fp.Image,
      Category = fp.Category,
      MerchantId = 0, // Seeded products are system-owned
      Status = "Active"
    }).ToList();

    // Adding more products to fulfill "add a lot of products more"
    var additionalProducts = new List<PrimeBasket.Product.API.Entities.Product>();
    string[] extraCats = { "Electronics", "Clothing", "Home Decor", "Fitness" };
    
    for (int i = 1; i <= 30; i++) {
        var cat = extraCats[i % extraCats.Length];
        additionalProducts.Add(new PrimeBasket.Product.API.Entities.Product {
            Name = $"Premium {cat} Item {i}",
            Description = $"This is a high-quality product from our {cat} collection. It offers premium features and durability.",
            Price = 499 + (i * 50),
            Stock = 50,
            ImageUrl = $"https://picsum.photos/seed/pb{i}/400/300",
            Category = cat,
            MerchantId = 0,
            Status = "Active"
        });
    }
    
    productsToAdd.AddRange(additionalProducts);

    await _context.Products.AddRangeAsync(productsToAdd);
    await _context.SaveChangesAsync();

    return productsToAdd.Count;
  }
}