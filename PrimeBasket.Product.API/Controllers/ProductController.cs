using Microsoft.AspNetCore.Mvc;
using PrimeBasket.Product.API.DTOs;
using PrimeBasket.Product.API.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace PrimeBasket.Product.API.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
  private readonly IProductService _service;

  public ProductController(IProductService service)
  {
    _service = service;
  }

  [AllowAnonymous]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var products = await _service.GetAllAsync();
    return Ok(products);
  }

  [AllowAnonymous]
  [HttpGet("{id}/stock")]
  public async Task<IActionResult> GetStock(int id)
  {
    var product = await _service.GetByIdAsync(id);

    if (product == null)
      return NotFound();

    return Ok(product.Stock);
  }

  [AllowAnonymous]
  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(int id)
  {
    var product = await _service.GetByIdAsync(id);

    if (product == null)
      return NotFound();

    return Ok(product);
  }

  [Authorize(Roles = "Admin,admin")]
  [HttpPost]
  public async Task<IActionResult> Add(ProductRequest request)
  {
    var product = await _service.AddProductAsync(request);
    return Ok(product);
  }

  [Authorize(Roles = "Admin,admin")]
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(int id, ProductRequest request)
  {
    var product = await _service.UpdateProductAsync(id, request);
    if (product == null) return NotFound("Product not found");
    return Ok(product);
  }

  [Authorize(Roles = "Admin,admin")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _service.DeleteProductAsync(id);
    if (!result) return NotFound("Product not found");
    return NoContent();
  }
}