using NUnit.Framework;
using PrimeBasket.Auth.API.Services.Auth;

namespace PrimeBasket.UnitTests.AuthTests;

[TestFixture]
public class PasswordHasherTests
{
    private PasswordHasher _passwordHasher;

    [SetUp]
    public void Setup()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Test]
    public void Hash_ShouldReturnHashedString()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        Assert.IsNotNull(hash);
        Assert.IsNotEmpty(hash);
        Assert.That(hash, Is.Not.EqualTo(password));
    }

    [Test]
    public void Verify_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        var password = "TestPassword123";
        var wrongPassword = "WrongPassword123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hash);

        // Assert
        Assert.IsFalse(result);
    }
}
