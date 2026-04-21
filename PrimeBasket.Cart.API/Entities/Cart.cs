namespace PrimeBasket.Cart.API.Entities;

public class Cart
{
  public int Id { get; set; }

  public int UserId { get; set; }   // 👈 from JWT

  public List<CartItem> Items { get; set; } = new();
}