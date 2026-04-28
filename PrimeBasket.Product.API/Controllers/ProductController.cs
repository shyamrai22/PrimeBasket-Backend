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

  // -------------------- GET ALL --------------------
  [AllowAnonymous]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var products = await _service.GetAllAsync();
    return Ok(products);
  }

  // -------------------- GET BY ID --------------------
  [AllowAnonymous]
  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(int id)
  {
    var product = await _service.GetByIdAsync(id);

    if (product == null)
      return NotFound("Product not found");

    return Ok(product);
  }

  // -------------------- GET STOCK --------------------
  [AllowAnonymous]
  [HttpGet("{id}/stock")]
  public async Task<IActionResult> GetStock(int id)
  {
    var product = await _service.GetByIdAsync(id);

    if (product == null)
      return NotFound("Product not found");

    return Ok(product.Stock);
  }

  // -------------------- ADD PRODUCT --------------------
  [Authorize(Roles = "Merchant,Admin")]
  [HttpPost]
  public async Task<IActionResult> Add(ProductRequest request)
  {
    var product = await _service.AddProductAsync(request);
    return Ok(product);
  }

  // -------------------- UPDATE PRODUCT --------------------
  [Authorize(Roles = "Merchant,Admin")]
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(int id, ProductRequest request)
  {
    var product = await _service.UpdateProductAsync(id, request);

    if (product == null)
      return NotFound("Product not found");

    return Ok(product);
  }

  // -------------------- DELETE PRODUCT --------------------
  [Authorize(Roles = "Merchant,Admin")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _service.DeleteProductAsync(id);

    if (!result)
      return NotFound("Product not found");

    return NoContent();
  }

  // -------------------- REDUCE STOCK --------------------
  public class ReduceStockRequest
  {
    public int Quantity { get; set; }
  }

  [Authorize]
  [HttpPost("{id}/reduce-stock")]
  public async Task<IActionResult> ReduceStock(int id, [FromBody] ReduceStockRequest request)
  {
    var product = await _service.GetByIdAsync(id);

    if (product == null)
      return NotFound("Product not found");

    if (request.Quantity <= 0)
      return BadRequest("Invalid quantity");

    if (product.Stock < request.Quantity)
      return BadRequest($"Not enough stock. Available: {product.Stock}");

    // CORE LOGIC
    product.Stock -= request.Quantity;

    await _service.UpdateStockAsync(product);

    return Ok("Stock reduced successfully");
  }

  [Authorize]
  [HttpPost("{id}/add-stock")]
  public async Task<IActionResult> AddStock(int id, [FromBody] ReduceStockRequest request)
  {
    var product = await _service.GetByIdAsync(id);

    if (product == null)
      return NotFound("Product not found");

    product.Stock += request.Quantity;

    await _service.UpdateStockAsync(product);

    return Ok("Stock restored");
  }

  // -------------------- SEED PRODUCTS --------------------
  [AllowAnonymous]
  [HttpPost("seed")]
  public async Task<IActionResult> SeedProducts()
  {
    var count = await _service.SeedProductsAsync();

    if (count == 0)
      return Ok("Database is already seeded or no products found.");

    return Ok($"Successfully seeded {count} products from FakeStoreAPI!");
  }
}