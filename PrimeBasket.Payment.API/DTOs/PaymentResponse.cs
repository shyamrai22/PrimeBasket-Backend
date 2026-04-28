namespace PrimeBasket.Payments.API.DTOs;

public class PaymentResponse
{
  public int PaymentId { get; set; }

  public int OrderId { get; set; }

  public decimal Amount { get; set; }

  public string PaymentMethod { get; set; } = string.Empty;

  public string Status { get; set; } = string.Empty;
  // Success, Failed, Pending

  public string Message { get; set; } = string.Empty;
}