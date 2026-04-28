namespace PrimeBasket.Payments.API.Entities;

public class PaymentModel
{
  public int PaymentModelId { get; set; }

  public int UserId { get; set; }
  public int OrderId { get; set; }

  public decimal Amount { get; set; }

  public string Status { get; set; } = string.Empty;

  public string PaymentMethod { get; set; } = string.Empty;

  public string IdempotencyKey { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}