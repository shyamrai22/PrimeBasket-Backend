using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeBasket.Payments.API.DTOs;
using PrimeBasket.Payments.API.Interfaces;
using System.Security.Claims;

namespace PrimeBasket.Payments.API.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
  private readonly IPaymentService _service;

  public PaymentController(IPaymentService service)
  {
    _service = service;
  }

  private int GetUserId()
  {
    return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
  }


  [HttpPost("pay")]
  public async Task<IActionResult> Pay([FromBody] PaymentRequest request)
  {
    var userId = GetUserId();

    var result = await _service.ProcessPaymentAsync(userId, request);

    if (result.Status == "Failed")
      return BadRequest(result);

    return Ok(result);
  }


  [HttpPost("wallet/create")]
  public async Task<IActionResult> CreateWallet()
  {
    var userId = GetUserId();

    var result = await _service.CreateWalletAsync(userId);

    return Ok(result);
  }

  [HttpGet("wallet")]
  public async Task<IActionResult> GetWallet()
  {
    var userId = GetUserId();

    var result = await _service.GetWalletByUserIdAsync(userId);

    return Ok(result);
  }

  [HttpPost("wallet/add-money")]
  public async Task<IActionResult> AddMoney([FromBody] AddMoneyRequest request)
  {
    var userId = GetUserId();

    var result = await _service.AddMoneyAsync(userId, request);

    return Ok(result);
  }


  [HttpGet("transactions")]
  public async Task<IActionResult> GetTransactions()
  {
    var userId = GetUserId();

    var result = await _service.GetTransactionsAsync(userId);

    return Ok(result);
  }

  // ---------------- RAZORPAY INTEGRATION ----------------

  [HttpPost("wallet/recharge/create-order")]
  public async Task<IActionResult> CreateRazorpayOrder([FromBody] RazorpayOrderRequest request)
  {
    var userId = GetUserId();
    var order = await _service.CreateRazorpayOrderAsync(userId, request);
    return Ok(order);
  }

  [HttpPost("wallet/recharge/verify")]
  public async Task<IActionResult> VerifyRazorpayPayment([FromBody] RazorpayVerifyRequest request)
  {
    try
    {
      var userId = GetUserId();
      var wallet = await _service.VerifyRazorpayPaymentAsync(userId, request);
      return Ok(wallet);
    }
    catch (Exception ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }
}