namespace PrimeBasket.Orders.API.DTOs;

public class OrderResponse
{
  public int OrderId { get; set; }

  public int UserId { get; set; }

  public DateTime CreatedAt { get; set; }

  public string Status { get; set; } = string.Empty;

  public decimal TotalAmount { get; set; }

  public int PaymentId { get; set; }

  public string PaymentMethod { get; set; } = string.Empty;

  public List<OrderItemResponse> Items { get; set; } = new();
}