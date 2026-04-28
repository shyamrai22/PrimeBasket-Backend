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

  // ---------------- CHECKOUT ----------------
  [HttpPost("checkout")]
  public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
  {
    var userId = GetUserId();

    var result = await _service.CheckoutAsync(userId, request);

    return Ok(result);
  }

  // ---------------- BUY NOW ----------------
  [HttpPost("buy-now")]
  public async Task<IActionResult> BuyNow([FromBody] BuyNowRequest request)
  {
    var userId = GetUserId();

    var result = await _service.BuyNowAsync(userId, request);

    return Ok(result);
  }

  // ---------------- GET ORDERS ----------------
  [HttpGet]
  public async Task<IActionResult> GetUserOrders()
  {
    var userId = GetUserId();

    var orders = await _service.GetUserOrdersAsync(userId);

    return Ok(orders);
  }

  // ---------------- UPDATE STATUS (ADMIN) ----------------
  [Authorize(Roles = "Admin")]
  [HttpPut("{orderId}/status")]
  public async Task<IActionResult> UpdateStatus(
      int orderId,
      [FromBody] UpdateOrderStatusRequest request)
  {
    var result = await _service.UpdateOrderStatusAsync(orderId, request);

    return Ok(result);
  }

  [Authorize(Roles = "Admin")]
  [HttpGet("admin/stats")]
  public async Task<IActionResult> GetAdminStats()
  {
    var stats = await _service.GetAdminStatsAsync();
    return Ok(stats);
  }

  [Authorize(Roles = "Admin")]
  [HttpGet("admin/all")]
  public async Task<IActionResult> GetAllOrders()
  {
    var orders = await _service.GetAllOrdersAsync();
    return Ok(orders);
  }

  [HttpPost("{orderId}/cancel")]
  public async Task<IActionResult> CancelOrder(int orderId)
  {
    var userId = GetUserId();
    var result = await _service.CancelOrderAsync(userId, orderId);
    return Ok(result);
  }
}