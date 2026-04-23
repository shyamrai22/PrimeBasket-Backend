namespace PrimeBasket.Orders.API.DTOs;

public class CartResponse
{
  public int Id { get; set; }
  public int UserId { get; set; }

  public List<CartItemResponse> Items { get; set; } = new();
}

public class CartItemResponse
{
  public int ProductId { get; set; }
  public int Quantity { get; set; }
}