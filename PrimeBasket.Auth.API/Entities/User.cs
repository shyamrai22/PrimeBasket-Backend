namespace PrimeBasket.Auth.API.Entities;

public class User
{
  public int Id { get; set; }

  public string FullName { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public string PasswordHash { get; set; } = string.Empty;

  public string Role { get; set; } = "Customer"; // Customer, Merchant, Admin

  public string Status { get; set; } = "Approved"; // Pending, Approved, Rejected
  public string? BusinessName { get; set; }
  public string? BusinessType { get; set; }
  public string? StoreDescription { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}