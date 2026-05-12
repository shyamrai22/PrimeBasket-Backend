using System.ComponentModel.DataAnnotations;

namespace PrimeBasket.Orders.API.DTOs;

public class BuyNowRequest
{
  [Required]
  public int ProductId { get; set; }

  [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
  public int Quantity { get; set; }

  [Required]
  [RegularExpression("^(Wallet|COD)$", ErrorMessage = "PaymentMethod must be 'Wallet' or 'COD'")]
  public string PaymentMethod { get; set; } = string.Empty;
}