using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PrimeBasket.Product.API.Data;
using PrimeBasket.Product.API.DTOs;
using PrimeBasket.Product.API.Entities;
using PrimeBasket.Product.API.Services;

namespace PrimeBasket.UnitTests.ProductTests;

[TestFixture]
public class ProductServiceTests
{
    private ProductDbContext _context;
    private ProductService _productService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: "ProductTestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);
        _productService = new ProductService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddProductAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var request = new ProductRequest
        {
            Name = "Test Product",
            Description = "Description",
            Price = 100,
            Stock = 10,
            ImageUrl = "url",
            Category = "Category"
        };
        var merchantId = 1;

        // Act
        var result = await _productService.AddProductAsync(request, merchantId);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Name, Is.EqualTo(request.Name));
        Assert.That(result.MerchantId, Is.EqualTo(merchantId));
        
        var productInDb = await _context.Products.FindAsync(result.Id);
        Assert.IsNotNull(productInDb);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Arrange
        _context.Products.Add(new PrimeBasket.Product.API.Entities.Product { Name = "P1", Description = "D1", Price = 10, MerchantId = 1, Category = "C1", Status = "Active" });
        _context.Products.Add(new PrimeBasket.Product.API.Entities.Product { Name = "P2", Description = "D2", Price = 20, MerchantId = 1, Category = "C2", Status = "Active" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        var product = new PrimeBasket.Product.API.Entities.Product { Name = "P1", Description = "D1", Price = 10, MerchantId = 1, Category = "C1", Status = "Active" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _productService.GetByIdAsync(product.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Name, Is.EqualTo("P1"));
    }

    [Test]
    public async Task UpdateProductAsync_ShouldUpdateDetails_WhenExists()
    {
        // Arrange
        var product = new PrimeBasket.Product.API.Entities.Product { Name = "Old Name", Description = "Old Desc", Price = 10, MerchantId = 1, Category = "C1", Status = "Active" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new ProductRequest { Name = "New Name", Description = "New Desc", Price = 20, Stock = 5, ImageUrl = "new-url", Category = "C2" };

        // Act
        var result = await _productService.UpdateProductAsync(product.Id, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Name, Is.EqualTo("New Name"));
        Assert.That(result.Price, Is.EqualTo(20));
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldUpdateStatus_WhenExists()
    {
        // Arrange
        var product = new PrimeBasket.Product.API.Entities.Product { Name = "P1", Description = "D1", Price = 10, MerchantId = 1, Category = "C1", Status = "Active" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _productService.UpdateStatusAsync(product.Id, "Inactive");

        // Assert
        Assert.IsTrue(result);
        var productInDb = await _context.Products.FindAsync(product.Id);
        Assert.That(productInDb.Status, Is.EqualTo("Inactive"));
    }

    [Test]
    public async Task GetByMerchantIdAsync_ShouldReturnOnlyMerchantProducts()
    {
        // Arrange
        _context.Products.Add(new PrimeBasket.Product.API.Entities.Product { Name = "M1-P1", MerchantId = 1, Category = "C1", Status = "Active", Description = "D", Price = 10 });
        _context.Products.Add(new PrimeBasket.Product.API.Entities.Product { Name = "M1-P2", MerchantId = 1, Category = "C1", Status = "Active", Description = "D", Price = 10 });
        _context.Products.Add(new PrimeBasket.Product.API.Entities.Product { Name = "M2-P1", MerchantId = 2, Category = "C1", Status = "Active", Description = "D", Price = 10 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _productService.GetByMerchantIdAsync(1);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.IsTrue(result.All(p => p.MerchantId == 1));
    }
}
