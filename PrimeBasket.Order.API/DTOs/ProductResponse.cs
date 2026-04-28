namespace PrimeBasket.Orders.API.DTOs;

public class ProductResponse
{
  public int ProductId { get; set; }

  public string Name { get; set; } = string.Empty;

  public decimal Price { get; set; }
}