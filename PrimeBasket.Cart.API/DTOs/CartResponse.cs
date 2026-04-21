namespace PrimeBasket.Cart.API.DTOs;

public class CartResponse
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public List<CartItemResponse> Items { get; set; } = new();
}