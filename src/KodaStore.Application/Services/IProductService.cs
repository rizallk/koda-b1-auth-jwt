using KodaStore.Application.DTOs;

namespace KodaStore.Application.Services;

public interface IProductService
{
  Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);
  Task<ProductResponse?> GetByCodeAsync(string productCode, CancellationToken cancellationToken);
  Task<ProductResponse?> CreateAsync(ProductRequest request, CancellationToken cancellationToken);
  Task<ProductResponse?> UpdateAsync(string productCode, UpdateProductRequest request, CancellationToken cancellationToken);

  Task<bool> DeleteAsync(string productCode, CancellationToken cancellationToken);
}