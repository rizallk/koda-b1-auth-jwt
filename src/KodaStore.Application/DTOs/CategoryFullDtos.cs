using System.ComponentModel.DataAnnotations;

namespace KodaStore.Application.DTOs;

public sealed class CreateCategoryWithProductsRequest : IValidatableObject
{
  [Required, StringLength(30)]
  public string Name { get; set; } = string.Empty;

  [Required, MinLength(1), MaxLength(20)]
  public List<string> ProductCodes { get; set; } = [];

  public IEnumerable<ValidationResult> Validate(ValidationContext context)
  {
    if (ProductCodes is null) yield break;
    if (ProductCodes.Any(code => string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30))
      yield return new ValidationResult("Setiap ProductCode wajib diisi, maksimal 30 karakter.", [nameof(ProductCodes)]);
    if (ProductCodes.Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim().ToUpperInvariant())
                    .Distinct(StringComparer.Ordinal).Count() != ProductCodes.Count)
      yield return new ValidationResult("ProductCode tidak boleh diulang.", [nameof(ProductCodes)]);
  }
}

public sealed record CategoryProductResponse(
    string ProductCode, string Name, decimal Price, int Stock, string CategoryName);

public sealed record CategoryProductsResponse(
    int CategoryId, string CategoryName, int Page, int PageSize,
    long TotalCount, IReadOnlyList<CategoryProductResponse> Items);

public sealed record CategoryCreatedResponse(int CategoryId, string Name,
    IReadOnlyList<string> AssignedProductCodes);

public enum CategoryCreateStatus { Created, DuplicateName, ProductNotFound, Conflict }
public sealed record CategoryCreateResult(
    CategoryCreateStatus Status,
    CategoryCreatedResponse? Data = null,
    string? Detail = null);

public sealed class AssignProductCategoryRequest
{
  [Range(1, int.MaxValue)]
  public int NewCategoryId { get; set; }
  public int? ExpectedCategoryId { get; set; }
}
public enum CategoryAssignStatus { Updated, ProductNotFound, CategoryNotFound, Conflict }

public sealed record ProductCategoryStateResponse(string ProductCode, int? CategoryId, string? CategoryName, string RowVersionBase64);