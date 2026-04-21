using System.ComponentModel.DataAnnotations;

namespace PrimeBasket.Cart.API.DTOs;

public class AddToCartRequest
{
  [Required]
  public int ProductId { get; set; }

  [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
  public int Quantity { get; set; }
}