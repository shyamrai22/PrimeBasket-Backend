using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PrimeBasket.Cart.API.DTOs;
using PrimeBasket.Cart.API.Interfaces;

namespace PrimeBasket.Cart.API.Controllers;

[Authorize] // Cart requires login
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
    var userId = GetUserId();
    var cart = await _service.AddToCartAsync(userId, request);
    return Ok(cart);
  }
}