using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PrimeBasket.Cart.API.DTOs;
using PrimeBasket.Cart.API.Interfaces;
using PrimeBasket.Cart.API.Exceptions;

namespace PrimeBasket.Cart.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
  private readonly ICartService _service;

  public CartController(ICartService service)
  {
    _service = service;
  }

  private int GetUserId()
  {
    return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
  }

  [Authorize]
  [HttpGet]
  public async Task<IActionResult> GetCart()
  {
    var userId = GetUserId();
    var cart = await _service.GetCartAsync(userId);
    return Ok(cart);
  }

  [HttpPost]
  public async Task<IActionResult> AddToCart(AddToCartRequest request)
  {
    try
    {
      var userId = GetUserId();
      var cart = await _service.AddToCartAsync(userId, request);
      return Ok(cart);
    }
    catch (NotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  [Authorize]
  [HttpDelete]
  public async Task<IActionResult> ClearCart()
  {
    var userId = GetUserId();
    await _service.ClearCartAsync(userId);
    return Ok("Cart cleared");
  }

  [Authorize]
  [HttpPut("{productId}")]
  public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] UpdateQuantityRequest request)
  {
    try
    {
      var userId = GetUserId();
      var cart = await _service.UpdateQuantityAsync(userId, productId, request.Quantity);
      return Ok(cart);
    }
    catch (NotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  [Authorize]
  [HttpDelete("{productId}")]
  public async Task<IActionResult> RemoveItem(int productId)
  {
    try
    {
      var userId = GetUserId();
      var cart = await _service.RemoveItemAsync(userId, productId);
      return Ok(cart);
    }
    catch (NotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }
}