namespace PrimeBasket.Payments.API.DTOs;

public class TransactionResponse
{
  public int TransactionId { get; set; }

  public decimal Amount { get; set; }

  public string Type { get; set; } = string.Empty;
  // CREDIT / DEBIT

  public string Status { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }
}