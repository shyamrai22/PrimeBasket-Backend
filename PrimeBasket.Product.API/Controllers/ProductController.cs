using Microsoft.AspNetCore.Mvc;
using PrimeBasket.Product.API.DTOs;
using PrimeBasket.Product.API.Interfaces;

namespace PrimeBasket.Product.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
  private readonly IProductService _service;

  public ProductController(IProductService service)
  {
    _service = service;
  }

  [HttpPost]
  public async Task<IActionResult> Add(ProductRequest request)
  {
    var product = await _service.AddProductAsync(request);
    return Ok(product);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var products = await _service.GetAllAsync();
    return Ok(products);
  }
}