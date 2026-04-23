using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PrimeBasket.Orders.API.Interfaces;
using PrimeBasket.Orders.API.DTOs;

namespace PrimeBasket.Orders.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
  private readonly IOrderService _service;

  public OrderController(IOrderService service)
  {
    _service = service;
  }

  private int GetUserId()
  {
    return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
  }

  [HttpPost("checkout")]
  public async Task<IActionResult> Checkout()
  {
    var userId = GetUserId();

    var result = await _service.CheckoutAsync(userId);

    return Ok(result);
  }

  [Authorize]
  [HttpPost("buy-now")]
  public async Task<IActionResult> BuyNow(BuyNowRequest request)
  {
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    var result = await _service.BuyNowAsync(userId, request);

    return Ok(result);
  }

  [Authorize]
  [HttpGet]
  public async Task<IActionResult> GetUserOrders()
  {
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    var orders = await _service.GetUserOrdersAsync(userId);

    return Ok(orders);
  }
}