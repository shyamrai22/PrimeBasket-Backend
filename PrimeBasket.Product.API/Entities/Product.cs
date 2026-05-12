namespace PrimeBasket.Product.API.Entities;

public class Product
{
  public int Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public decimal Price { get; set; }

  public int Stock { get; set; }

  public string ImageUrl { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;

  public int MerchantId { get; set; } // The ID of the merchant who owns this product
  public string Status { get; set; } = "Active"; // Active, Inactive, Flagged
}