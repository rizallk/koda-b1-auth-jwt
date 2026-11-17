using KodaStore.Application.DTOs;

namespace KodaStore.Application.Services;

public interface IProductQueryService
{
  Task<PagedResult<ProductSearchItemResponse>> SearchAsync(
      ProductSearchRequest request, CancellationToken ct
  );
}