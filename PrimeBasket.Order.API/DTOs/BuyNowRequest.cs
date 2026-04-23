namespace PrimeBasket.Orders.API.DTOs;

public class BuyNowRequest
{
  public int ProductId { get; set; }
  public int Quantity { get; set; }
}