namespace PrimeBasket.Orders.API.DTOs;

public class CreateOrderRequest
{
  public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
  public int ProductId { get; set; }
  public int Quantity { get; set; }
}