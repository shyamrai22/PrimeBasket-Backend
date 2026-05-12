namespace PrimeBasket.Auth.API.DTOs;

public class UserDto
{
  public int Id { get; set; }
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Role { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public string? BusinessName { get; set; }
  public string? BusinessType { get; set; }
  public string? StoreDescription { get; set; }
  public DateTime CreatedAt { get; set; }
}
