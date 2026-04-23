namespace PrimeBasket.Orders.API.DTOs;

public class OrderResponse
{
  public int OrderId { get; set; }
  public int UserId { get; set; }
  public DateTime CreatedAt { get; set; }

  public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
  public int ProductId { get; set; }
  public int Quantity { get; set; }
}