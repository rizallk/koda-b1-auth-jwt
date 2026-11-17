using KodaStore.Application.DTOs;

namespace KodaStore.Application.Services;

public interface ICategoryService
{
  Task<CategoryCreateResult> CreateWithProductsAsync(CreateCategoryWithProductsRequest request, CancellationToken ct);

  Task<CategoryProductsResponse?> GetProductsAsync(int categoryId, int page, int pageSize, CancellationToken ct);

  Task<ProductCategoryStateResponse?> GetProductsAssignmentAsync(string productCode, CancellationToken ct);

  Task<CategoryAssignStatus> AssignProductsAsync(string productCode, AssignProductCategoryRequest request, CancellationToken ct);
}