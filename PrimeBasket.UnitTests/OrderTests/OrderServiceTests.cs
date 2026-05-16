using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PrimeBasket.Orders.API.Data;
using PrimeBasket.Orders.API.DTOs;
using PrimeBasket.Orders.API.Entities;
using OrderEntity = PrimeBasket.Orders.API.Entities.Order;
using PrimeBasket.Orders.API.Enums;
using PrimeBasket.Orders.API.Services;

namespace PrimeBasket.UnitTests.OrderTests;

[TestFixture]
public class OrderServiceTests
{
    private OrderDbContext _context;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<PrimeBasket.Common.Messaging.IMessageProducer> _mockMessageProducer;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderTestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        // Provide default HttpClients with BaseAddress to avoid URI errors
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK)) { BaseAddress = new Uri("http://localhost") });

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer test-token";
        _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        _mockMessageProducer = new Mock<PrimeBasket.Common.Messaging.IMessageProducer>();
    }

    private OrderService CreateService()
    {
        return new OrderService(_context, _mockHttpClientFactory.Object, _mockHttpContextAccessor.Object, _mockMessageProducer.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetUserOrdersAsync_ShouldReturnOrders_ForSpecificUser()
    {
        // Arrange
        var service = CreateService();
        var userId = 1;
        _context.Orders.Add(new OrderEntity { UserId = userId, TotalAmount = 100, Status = OrderStatus.Paid, PaymentMethod = "Wallet", Items = new List<OrderItem>() });
        _context.Orders.Add(new OrderEntity { UserId = userId, TotalAmount = 200, Status = OrderStatus.Pending, PaymentMethod = "COD", Items = new List<OrderItem>() });
        _context.Orders.Add(new OrderEntity { UserId = 2, TotalAmount = 300, Status = OrderStatus.Paid, PaymentMethod = "Wallet", Items = new List<OrderItem>() });
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetUserOrdersAsync(userId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.IsTrue(result.All(o => o.UserId == userId));
    }

    [Test]
    public async Task GetAdminStatsAsync_ShouldCalculateCorrectStats()
    {
        // Arrange
        var service = CreateService();
        _context.Orders.Add(new OrderEntity { TotalAmount = 100, Status = OrderStatus.Paid, PaymentMethod = "Wallet", Items = new List<OrderItem>() });
        _context.Orders.Add(new OrderEntity { TotalAmount = 200, Status = OrderStatus.Pending, PaymentMethod = "Wallet", Items = new List<OrderItem>() });
        _context.Orders.Add(new OrderEntity { TotalAmount = 300, Status = OrderStatus.Cancelled, PaymentMethod = "Wallet", Items = new List<OrderItem>() });
        _context.Orders.Add(new OrderEntity { TotalAmount = 400, Status = OrderStatus.Delivered, PaymentMethod = "Wallet", Items = new List<OrderItem>() });
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetAdminStatsAsync();

        // Assert
        Assert.That(result.TotalOrders, Is.EqualTo(4));
        Assert.That(result.PendingOrders, Is.EqualTo(1));
        Assert.That(result.CancelledOrders, Is.EqualTo(1));
        Assert.That(result.TotalRevenue, Is.EqualTo(700));
    }

    [Test]
    public async Task GetMerchantOrdersAsync_ShouldReturnOnlyOrdersWithMerchantItems()
    {
        // Arrange
        var service = CreateService();
        var merchantId = 10;
        var order = new OrderEntity
        {
            UserId = 1,
            TotalAmount = 500,
            Status = OrderStatus.Paid,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, MerchantId = merchantId, Quantity = 1, Price = 300 },
                new OrderItem { ProductId = 2, MerchantId = 20, Quantity = 1, Price = 200 }
            }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetMerchantOrdersAsync(merchantId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Items.Count, Is.EqualTo(1));
        Assert.That(result[0].Items[0].MerchantId, Is.EqualTo(merchantId));
        Assert.That(result[0].TotalAmount, Is.EqualTo(300));
    }

    [Test]
    public async Task UpdateOrderStatusAsync_ShouldChangeStatus_WhenOrderExists()
    {
        // Arrange
        var service = CreateService();
        var order = new OrderEntity { UserId = 1, TotalAmount = 100, Status = OrderStatus.Pending, Items = new List<OrderItem>() };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var request = new UpdateOrderStatusRequest { Status = "Shipped" };

        // Act
        var result = await service.UpdateOrderStatusAsync(order.Id, request);

        // Assert
        Assert.That(result, Is.EqualTo("Order status updated to Shipped"));
        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        Assert.That(updatedOrder.Status, Is.EqualTo(OrderStatus.Shipped));
    }

    [Test]
    public async Task CancelOrderAsync_ShouldSucceed_WhenOrderIsPaidViaWallet()
    {
        // Arrange
        var userId = 1;
        var order = new OrderEntity 
        { 
            UserId = userId, 
            TotalAmount = 500, 
            Status = OrderStatus.Paid, 
            PaymentMethod = "Wallet",
            Items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 1, Price = 500 } }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var service = CreateService();

        // Act
        var result = await service.CancelOrderAsync(userId, order.Id);

        // Assert
        Assert.That(result, Does.Contain("cancelled successfully"));
        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        Assert.That(updatedOrder.Status, Is.EqualTo(OrderStatus.Cancelled));
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        public MockHttpMessageHandler(HttpStatusCode statusCode) => _statusCode = statusCode;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}
