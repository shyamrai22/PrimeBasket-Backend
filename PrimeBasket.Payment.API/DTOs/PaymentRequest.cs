namespace PrimeBasket.Payments.API.DTOs;

public class PaymentRequest
{
  public int OrderId { get; set; }

  public decimal Amount { get; set; }

  public string PaymentMethod { get; set; } = string.Empty;
  // "Wallet" or "COD"

  public string IdempotencyKey { get; set; } = string.Empty;
}