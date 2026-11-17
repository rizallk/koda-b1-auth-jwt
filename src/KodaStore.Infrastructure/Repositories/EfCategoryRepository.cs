
using KodaStore.Application.DTOs;
using KodaStore.Application.Repositories;
using KodaStore.Domain.Entities;
using KodaStore.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KodaStore.Infrastructure.Repositories;

public sealed class EfCategoryRepository : ICategoryRepository
{
  private readonly AppDbContext _db;
  private readonly ILogger<EfCategoryRepository> _logger;

  public EfCategoryRepository(AppDbContext db, ILogger<EfCategoryRepository> logger)
  {
    _db = db;
    _logger = logger;
  }

  public async Task<CategoryCreateResult> CreateWithProductAsync(
        string name, IReadOnlyList<string> productCodes, CancellationToken ct)
  {
    await using var transaction = await _db.Database.BeginTransactionAsync(ct);
    try
    {
      if (await _db.Categories.AnyAsync(c => c.Name == name, ct))
        return new CategoryCreateResult(CategoryCreateStatus.DuplicateName,
            Detail: "Nama kategori sudah terdaftar.");

      var category = new Category(name);
      _db.Categories.Add(category);
      await _db.SaveChangesAsync(ct);

      var products = await _db.Products
          .Where(p => productCodes.Contains(p.ProductCode))
          .ToListAsync(ct);

      if (products.Count != productCodes.Count)
      {
        await transaction.RollbackAsync(CancellationToken.None);
        return new CategoryCreateResult(CategoryCreateStatus.ProductNotFound,
            Detail: "Satu atau beberapa ProductCode tidak ditemukan.");
      }

      foreach (var product in products)
        product.AssignCategory(category);

      await _db.SaveChangesAsync(ct); // UPDATE Product (RowVersion dijaga EF Core).
      await transaction.CommitAsync(ct); // Baru di sini semua perubahan final.

      return new CategoryCreateResult(CategoryCreateStatus.Created,
          new CategoryCreatedResponse(category.CategoryId, category.Name,
              productCodes.ToArray()));
    }
    catch (DbUpdateConcurrencyException ex)
    {
      await transaction.RollbackAsync(CancellationToken.None);
      _logger.LogWarning(ex, "Concurrent category assignment detected");
      return new CategoryCreateResult(CategoryCreateStatus.Conflict,
          Detail: "Produk berubah oleh request lain. Muat ulang data.");
    }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
    {
      await transaction.RollbackAsync(CancellationToken.None);
      return new CategoryCreateResult(CategoryCreateStatus.DuplicateName,
          Detail: "Kategori dengan nama ini sudah dibuat oleh request lain.");
    }
    catch
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
  }

  public async Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(
      string productCode, CancellationToken ct)
  {
    var state = await _db.Products.AsNoTracking()
        .Where(p => p.ProductCode == productCode)
        .Select(p => new
        {
          p.ProductCode,
          p.CategoryId,
          CategoryName = p.CategoryEntity == null ? null : p.CategoryEntity.Name,
          p.RowVersion
        })
        .SingleOrDefaultAsync(ct);
    return state is null ? null : new ProductCategoryStateResponse(
        state.ProductCode, state.CategoryId, state.CategoryName,
        Convert.ToBase64String(state.RowVersion));
  }

  public async Task<CategoryAssignStatus> AssignProductCompareAndSwapAsync(
      string productCode, int newCategoryId, int? expectedCategoryId,
      CancellationToken ct)
  {
    var newCategory = await _db.Categories.AsNoTracking()
        .Where(c => c.CategoryId == newCategoryId)
        .Select(c => new { c.Name })
        .SingleOrDefaultAsync(ct);
    if (newCategory is null) return CategoryAssignStatus.CategoryNotFound;

    int updated;
    try
    {
      updated = await _db.Products
          .Where(p => p.ProductCode == productCode && p.CategoryId == expectedCategoryId)
          .ExecuteUpdateAsync(setters => setters
              .SetProperty(p => p.CategoryId, newCategoryId)
              .SetProperty(p => p.Category, newCategory.Name), ct);
    }
    catch (SqlException ex) when (ex.Number == 547)
    {
      return CategoryAssignStatus.Conflict;
    }

    if (updated == 1) return CategoryAssignStatus.Updated;

    bool exists = await _db.Products.AsNoTracking()
        .AnyAsync(p => p.ProductCode == productCode, ct);
    return exists ? CategoryAssignStatus.Conflict : CategoryAssignStatus.ProductNotFound;
  }

  public async Task<CategoryProductsResponse?> GetProductsAsync(int categoryId, int page, int pageSize, CancellationToken ct)
  {
    var category = await _db.Categories.AsNoTracking()
        .Where(c => c.CategoryId == categoryId)
        .Select(c => new { c.CategoryId, c.Name })
        .SingleOrDefaultAsync(ct);

    if (category is null) return null;

    var query = _db.Products.AsNoTracking()
        .Where(p => p.CategoryId == categoryId);
    long totalCount = await query.LongCountAsync(ct);
    var items = await query.OrderBy(p => p.ProductCode)
        .Skip((page - 1) * pageSize).Take(pageSize)
        .Select(p => new CategoryProductResponse(
            p.ProductCode, p.Name, p.Price, p.Stock, p.CategoryEntity!.Name))
        .ToArrayAsync(ct);

    return new CategoryProductsResponse(category.CategoryId, category.Name,
        page, pageSize, totalCount, items);
  }
}