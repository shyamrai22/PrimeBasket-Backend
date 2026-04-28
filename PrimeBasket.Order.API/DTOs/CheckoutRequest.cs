using System.ComponentModel.DataAnnotations;

namespace PrimeBasket.Orders.API.DTOs;

public class CheckoutRequest
{
    [Required]
    [RegularExpression("^(Wallet|COD)$", ErrorMessage = "PaymentMethod must be 'Wallet' or 'COD'")]
    public string PaymentMethod { get; set; } = string.Empty;
}
