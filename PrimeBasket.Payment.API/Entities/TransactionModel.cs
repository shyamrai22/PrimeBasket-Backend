namespace PrimeBasket.Payments.API.Entities;

public class TransactionModel
{
  public int TransactionModelId { get; set; }

  public int WalletId { get; set; }

  public decimal Amount { get; set; }

  public string Type { get; set; } = string.Empty;

  public string Status { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public WalletModel Wallet { get; set; } = null!;
}