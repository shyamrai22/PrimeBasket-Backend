namespace PrimeBasket.Cart.API.DTOs;

public class AddToCartRequest
{
  public int ProductId { get; set; }
  public int Quantity { get; set; }
}