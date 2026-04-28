namespace PrimeBasket.Payments.API.Entities;

public class WalletModel
{
  public int WalletModelId { get; set; }

  public int UserId { get; set; }

  public decimal Balance { get; set; }

  public ICollection<TransactionModel> Transactions { get; set; }
      = new List<TransactionModel>();
}