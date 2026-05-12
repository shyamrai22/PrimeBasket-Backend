using PrimeBasket.Product.API.DTOs;
using PrimeBasket.Product.API.Entities;

namespace PrimeBasket.Product.API.Interfaces;

public interface IProductService
{
  Task<PrimeBasket.Product.API.Entities.Product> AddProductAsync(ProductRequest request, int merchantId);
  Task<List<PrimeBasket.Product.API.Entities.Product>> GetAllAsync();
  Task<List<PrimeBasket.Product.API.Entities.Product>> GetByMerchantIdAsync(int merchantId);
  Task<PrimeBasket.Product.API.Entities.Product?> UpdateProductAsync(int id, ProductRequest request);
  Task<bool> DeleteProductAsync(int id);
  Task<PrimeBasket.Product.API.Entities.Product?> GetByIdAsync(int id);
  Task UpdateStockAsync(PrimeBasket.Product.API.Entities.Product product);
  Task<int> SeedProductsAsync();
  Task<bool> UpdateStatusAsync(int productId, string status);
}