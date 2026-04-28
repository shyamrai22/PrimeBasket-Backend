using System.ComponentModel.DataAnnotations;

namespace PrimeBasket.Orders.API.DTOs;

public class PaymentRequestDto
{
  [Required]
  public int OrderId { get; set; }

  [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
  public decimal Amount { get; set; }

  [Required]
  [RegularExpression("^(Wallet|COD)$", ErrorMessage = "PaymentMethod must be 'Wallet' or 'COD'")]
  public string PaymentMethod { get; set; } = string.Empty;

  [Required]
  public string IdempotencyKey { get; set; } = string.Empty;
}