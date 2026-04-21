using PrimeBasket.Product.API.DTOs;

namespace PrimeBasket.Product.API.Interfaces;

public interface IProductService
{
  Task<PrimeBasket.Product.API.Entities.Product> AddProductAsync(ProductRequest request);
  Task<List<PrimeBasket.Product.API.Entities.Product>> GetAllAsync();
  Task<PrimeBasket.Product.API.Entities.Product?> UpdateProductAsync(int id, ProductRequest request);
  Task<bool> DeleteProductAsync(int id);
  Task<PrimeBasket.Product.API.Entities.Product?> GetByIdAsync(int id);
}