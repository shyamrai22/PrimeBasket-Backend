namespace PrimeBasket.Payments.API.DTOs;

public class WalletResponse
{
  public int WalletId { get; set; }

  public int UserId { get; set; }

  public decimal Balance { get; set; }
}