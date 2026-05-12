using PrimeBasket.Orders.API.Enums;

namespace PrimeBasket.Orders.API.Entities;

public class Order
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public List<OrderItem> Items { get; set; } = new();
  public int PaymentId { get; set; }
  public OrderStatus Status { get; set; } = OrderStatus.Pending;
  public decimal TotalAmount { get; set; }
  public string PaymentMethod { get; set; } = string.Empty;
}