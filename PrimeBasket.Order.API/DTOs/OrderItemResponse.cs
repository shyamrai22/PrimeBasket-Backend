namespace PrimeBasket.Orders.API.DTOs;

public class OrderItemResponse
{
  public int OrderItemId { get; set; }

  public int ProductId { get; set; }

  public int Quantity { get; set; }

  public decimal Price { get; set; }

  public decimal TotalPrice { get; set; }
}