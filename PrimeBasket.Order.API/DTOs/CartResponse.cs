using System.ComponentModel.DataAnnotations;

namespace PrimeBasket.Orders.API.DTOs;

public class CartResponse
{
  public int CartId { get; set; }

  public int UserId { get; set; }

  public List<CartItemResponse> Items { get; set; } = new();
}

public class CartItemResponse
{
  public int ProductId { get; set; }

  [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
  public int Quantity { get; set; }
}