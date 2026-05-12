using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using PrimeBasket.Cart.API.Data;
using PrimeBasket.Cart.API.DTOs;
using PrimeBasket.Cart.API.Entities;
using PrimeBasket.Cart.API.Services;

namespace PrimeBasket.UnitTests.CartTests;

[TestFixture]
public class CartServiceTests
{
    private CartDbContext _context;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private CartService _cartService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CartDbContext>()
            .UseInMemoryDatabase(databaseName: "CartTestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new CartDbContext(options);

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _cartService = new CartService(_context, _mockHttpClientFactory.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetCartAsync_ShouldReturnEmptyCart_WhenCartDoesNotExist()
    {
        // Act
        var result = await _cartService.GetCartAsync(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task AddToCartAsync_ShouldAddNewItem_WhenStockIsAvailable()
    {
        // Arrange
        var userId = 1;
        var productId = 101;
        var quantity = 2;
        var stock = 10;

        SetupStockResponse(productId, stock);

        var request = new AddToCartRequest { ProductId = productId, Quantity = quantity };

        // Act
        var result = await _cartService.AddToCartAsync(userId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.Items[0].ProductId, Is.EqualTo(productId));
        Assert.That(result.Items[0].Quantity, Is.EqualTo(quantity));
    }

    [Test]
    public async Task AddToCartAsync_ShouldThrowException_WhenStockIsInsufficient()
    {
        // Arrange
        var userId = 1;
        var productId = 101;
        var quantity = 20;
        var stock = 10;

        SetupStockResponse(productId, stock);

        var request = new AddToCartRequest { ProductId = productId, Quantity = quantity };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _cartService.AddToCartAsync(userId, request));
        Assert.That(ex.Message, Is.EqualTo($"Only {stock} items available in stock"));
    }

    [Test]
    public async Task UpdateQuantityAsync_ShouldUpdate_WhenItemExistsAndStockAvailable()
    {
        // Arrange
        var userId = 1;
        var productId = 101;
        var initialQty = 2;
        var newQty = 5;
        var stock = 10;

        _context.Carts.Add(new PrimeBasket.Cart.API.Entities.Cart 
        { 
            UserId = userId, 
            Items = new List<CartItem> { new CartItem { ProductId = productId, Quantity = initialQty } } 
        });
        await _context.SaveChangesAsync();

        SetupStockResponse(productId, stock);

        // Act
        var result = await _cartService.UpdateQuantityAsync(userId, productId, newQty);

        // Assert
        Assert.That(result.Items[0].Quantity, Is.EqualTo(newQty));
    }

    [Test]
    public async Task RemoveItemAsync_ShouldRemoveItem_WhenExists()
    {
        // Arrange
        var userId = 1;
        var productId = 101;
        _context.Carts.Add(new PrimeBasket.Cart.API.Entities.Cart 
        { 
            UserId = userId, 
            Items = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 2 } } 
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.RemoveItemAsync(userId, productId);

        // Assert
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task ClearCartAsync_ShouldRemoveCartAndItems()
    {
        // Arrange
        var userId = 1;
        _context.Carts.Add(new PrimeBasket.Cart.API.Entities.Cart 
        { 
            UserId = userId, 
            Items = new List<CartItem> { new CartItem { ProductId = 101, Quantity = 2 } } 
        });
        await _context.SaveChangesAsync();

        // Act
        await _cartService.ClearCartAsync(userId);

        // Assert
        var cartInDb = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.IsNull(cartInDb);
    }

    private void SetupStockResponse(int productId, int stock)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/api/products/{productId}/stock")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(stock)
            });
    }
}
