using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using PrimeBasket.Payments.API.Data;
using PrimeBasket.Payments.API.DTOs;
using PrimeBasket.Payments.API.Entities;
using PrimeBasket.Payments.API.Services;

namespace PrimeBasket.UnitTests.PaymentTests;

[TestFixture]
public class PaymentServiceTests
{
    private PaymentDbContext _context;
    private Mock<IConfiguration> _mockConfiguration;
    private PaymentService _paymentService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: "PaymentTestDb_" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new PaymentDbContext(options);
        _mockConfiguration = new Mock<IConfiguration>();

        _paymentService = new PaymentService(_context, _mockConfiguration.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddMoneyAsync_ShouldIncreaseBalanceAndCreateTransaction()
    {
        // Arrange
        var userId = 1;
        var amount = 500m;
        var request = new AddMoneyRequest { Amount = amount };

        // Act
        var result = await _paymentService.AddMoneyAsync(userId, request);

        // Assert
        Assert.That(result.Balance, Is.EqualTo(amount));
        
        var wallet = await _context.Wallets.Include(w => w.Transactions).FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.IsNotNull(wallet);
        Assert.That(wallet.Balance, Is.EqualTo(amount));
        Assert.That(wallet.Transactions.Count, Is.EqualTo(1));
        Assert.That(wallet.Transactions.First().Amount, Is.EqualTo(amount));
        Assert.That(wallet.Transactions.First().Type, Is.EqualTo("CREDIT"));
    }

    [Test]
    public async Task GetWalletByUserIdAsync_ShouldReturnWallet_WhenExists()
    {
        // Arrange
        var userId = 1;
        var wallet = new WalletModel { UserId = userId, Balance = 1000m };
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        // Act
        var result = await _paymentService.GetWalletByUserIdAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Balance, Is.EqualTo(1000m));
    }

    [Test]
    public void GetWalletByUserIdAsync_ShouldThrowException_WhenWalletDoesNotExist()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _paymentService.GetWalletByUserIdAsync(1));
        Assert.That(ex.Message, Is.EqualTo("Wallet not found"));
    }

    [Test]
    public async Task ProcessPaymentAsync_ShouldSucceed_ForCOD()
    {
        // Arrange
        var userId = 1;
        var request = new PaymentRequest 
        { 
            OrderId = 123, 
            Amount = 1000, 
            PaymentMethod = "COD", 
            IdempotencyKey = "key-cod" 
        };

        // Act
        var result = await _paymentService.ProcessPaymentAsync(userId, request);

        // Assert
        Assert.That(result.Status, Is.EqualTo("Pending"));
        Assert.That(result.PaymentMethod, Is.EqualTo("COD"));
        
        var paymentInDb = await _context.Payments.FirstOrDefaultAsync(p => p.IdempotencyKey == "key-cod");
        Assert.IsNotNull(paymentInDb);
    }

    [Test]
    public async Task ProcessPaymentAsync_ShouldFail_WhenWalletBalanceInsufficient()
    {
        // Arrange
        var userId = 1;
        _context.Wallets.Add(new WalletModel { UserId = userId, Balance = 100m });
        await _context.SaveChangesAsync();

        var request = new PaymentRequest 
        { 
            OrderId = 123, 
            Amount = 500, 
            PaymentMethod = "Wallet", 
            IdempotencyKey = "key-wallet-fail" 
        };

        // Act
        var result = await _paymentService.ProcessPaymentAsync(userId, request);

        // Assert
        Assert.That(result.Status, Is.EqualTo("Failed"));
        Assert.That(result.Message, Is.EqualTo("Insufficient balance"));
    }

    [Test]
    public async Task GetTransactionsAsync_ShouldReturnAllTransactionsForUser()
    {
        // Arrange
        var userId = 1;
        var wallet = new WalletModel { UserId = userId, Balance = 1000m };
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        _context.Transactions.Add(new TransactionModel { WalletId = wallet.WalletModelId, Amount = 500, Type = "CREDIT", Status = "Success", CreatedAt = DateTime.UtcNow });
        _context.Transactions.Add(new TransactionModel { WalletId = wallet.WalletModelId, Amount = 200, Type = "DEBIT", Status = "Success", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act
        var result = await _paymentService.GetTransactionsAsync(userId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
    }
}
