using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using PrimeBasket.Auth.API.Data;
using PrimeBasket.Auth.API.DTOs;
using PrimeBasket.Auth.API.Entities;
using PrimeBasket.Auth.API.Services.Auth;

namespace PrimeBasket.UnitTests.AuthTests;

[TestFixture]
public class AuthServiceTests
{
    private AuthDbContext _context;
    private Mock<IConfiguration> _mockConfig;
    private PasswordHasher _hasher;
    private TokenService _tokenService;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "AuthTestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);
        _mockConfig = new Mock<IConfiguration>();
        _hasher = new PasswordHasher();

        // Setup some basic config for TokenService
        _mockConfig.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKey12345678901234567890123456789012");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("PrimeBasket");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("PrimeBasketUsers");
        _mockConfig.Setup(c => c["AdminSettings:AdminKey"]).Returns("AdminKey123");
        _mockConfig.Setup(c => c["RoleSettings:MerchantKey"]).Returns("MerchantKey123");

        _tokenService = new TokenService(_mockConfig.Object);
        _authService = new AuthService(_context, _tokenService, _hasher, _mockConfig.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsNew()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            FullName = "Test User",
            Role = "Customer"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.EqualTo("User registered successfully"));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.IsNotNull(user);
        Assert.That(user.Role, Is.EqualTo("Customer"));
        Assert.That(user.Status, Is.EqualTo("Approved"));
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnError_WhenUserAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest { Email = "exists@example.com", Password = "Pwd", FullName = "Name" };
        _context.Users.Add(new User { Email = "exists@example.com", PasswordHash = "hash", FullName = "Name", Role = "Customer" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.EqualTo("User already exists"));
    }

    [Test]
    public async Task RegisterAsync_ShouldRegisterAdmin_WhenValidKeyProvided()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "admin@example.com",
            Password = "Password123",
            FullName = "Admin User",
            Role = "Admin",
            RoleKey = "AdminKey123"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.EqualTo("User registered successfully"));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
        Assert.That(user.Role, Is.EqualTo("Admin"));
    }

    [Test]
    public async Task RegisterAsync_ShouldRegisterMerchantAsPending_WhenValidKeyProvided()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "merchant@example.com",
            Password = "Password123",
            FullName = "Merchant User",
            Role = "Merchant",
            RoleKey = "MerchantKey123"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.EqualTo("User registered successfully"));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "merchant@example.com");
        Assert.That(user.Role, Is.EqualTo("Merchant"));
        Assert.That(user.Status, Is.EqualTo("Pending"));
    }

    [Test]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "login@example.com";
        var password = "Password123";
        var user = new User
        {
            Email = email,
            PasswordHash = _hasher.Hash(password),
            FullName = "Login User",
            Role = "Customer",
            Status = "Approved"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest { Email = email, Password = password };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.IsNotEmpty(result);
        Assert.That(result, Does.Contain(".")); // Basic JWT check
    }

    [Test]
    public async Task LoginAsync_ShouldReturnError_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "login@example.com";
        var user = new User { Email = email, PasswordHash = _hasher.Hash("CorrectPwd"), FullName = "User", Role = "Customer", Status = "Approved" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest { Email = email, Password = "WrongPassword" };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result, Is.EqualTo("Invalid credentials"));
    }

    [Test]
    public async Task UpdateUserStatusAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var user = new User { Email = "status@example.com", FullName = "User", PasswordHash = "h", Role = "Merchant", Status = "Pending" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.UpdateUserStatusAsync(user.Id, "Approved");

        // Assert
        Assert.IsTrue(result);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.That(updatedUser.Status, Is.EqualTo("Approved"));
    }
}
