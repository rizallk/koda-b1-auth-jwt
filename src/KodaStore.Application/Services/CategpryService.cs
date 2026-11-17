using KodaStore.Application.DTOs;
using KodaStore.Application.Repositories;

namespace KodaStore.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    public CategoryService(ICategoryRepository repository) => _repository = repository;

    public Task<CategoryCreateResult> CreateWithProductsAsync(
        CreateCategoryWithProductsRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 30)
            throw new ArgumentException("Nama kategori wajib diisi (maksimal 30 karakter).");
        if (request.ProductCodes is null || request.ProductCodes.Count is < 1 or > 20)
            throw new ArgumentException("Pilih antara 1 sampai 20 produk.");

        var codes = request.ProductCodes.Select(s => (s ?? string.Empty).Trim().ToUpperInvariant())
            .ToArray();
        if (codes.Any(s => string.IsNullOrWhiteSpace(s) || s.Length > 30)
            || codes.Distinct(StringComparer.Ordinal).Count() != codes.Length)
            throw new ArgumentException("ProductCode tidak valid atau duplikat.");

        return _repository.CreateWithProductAsync(request.Name.Trim(), codes, ct);
    }

    public Task<CategoryProductsResponse?> GetProductsAsync(int categoryId,
        int page, int pageSize, CancellationToken ct)
    {
        if (categoryId <= 0 || page <= 0 || pageSize is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(page), "Pagination atau CategoryId tidak valid.");
        return _repository.GetProductsAsync(categoryId, page, pageSize, ct);
    }

    public Task<ProductCategoryStateResponse?> GetProductsAssignmentAsync(
        string productCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30)
            throw new ArgumentException("ProductCode tidak valid.");
        return _repository.GetProductAssignmentAsync(productCode.Trim().ToUpperInvariant(), ct);
    }

    public Task<CategoryAssignStatus> AssignProductsAsync(string productCode, AssignProductCategoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30)
            throw new ArgumentException("ProductCode tidak valid.");
        if (request.NewCategoryId <= 0 || request.ExpectedCategoryId < 0)
            throw new ArgumentException("CategoryId tidak valid.");
        return _repository.AssignProductCompareAndSwapAsync(
            productCode.Trim().ToUpperInvariant(), request.NewCategoryId,
            request.ExpectedCategoryId, ct);
    }
}