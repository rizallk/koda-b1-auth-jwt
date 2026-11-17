using KodaStore.Application.DTOs;

namespace KodaStore.Application.Repositories;

public interface IProductQueryRepository
{
  Task<PagedResult<ProductSearchItemResponse>> SearchAsync(ProductSearchCriteria criteria, CancellationToken ct);
}