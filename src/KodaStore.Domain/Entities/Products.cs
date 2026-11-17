
namespace KodaStore.Domain.Entities;

public sealed class Product
{
  public string ProductCode { get; private set; }
  public string Name { get; private set; }
  public decimal Price { get; private set; }
  public int Stock { get; private set; }
  public string Category { get; private set; }
  public string? Description { get; private set; } = string.Empty;
  public bool? IsActive { get; private set; } = true;

  public DateTime? DeactivatedAtUtc { get; private set; } = null;

  public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

  public int? CategoryId { get; private set; }
  public Category? CategoryEntity { get; private set; }

  public byte[] RowVersion { get; private set; } = [];

  public Product(string productCode, string name, decimal price, int stock, string? category,
                 string? description = null, bool? isActive = true, DateTime? deactivatedAtUtc = null)
  {
    if (string.IsNullOrWhiteSpace(productCode))
    {
      throw new ArgumentException("Product code is required", nameof(productCode));
    }
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Name is required", nameof(name));
    }
    if (price < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
    }
    if (stock < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative");
    }
    if (string.IsNullOrWhiteSpace(category))
    {
      throw new ArgumentException("Category is required", nameof(category));
    }

    ProductCode = productCode;
    Name = name;
    Price = price;
    Stock = stock;
    Category = category.Trim();
    Description = description;
    IsActive = isActive;
    DeactivatedAtUtc = deactivatedAtUtc;
  }

  public void UpdateDetail(string name, decimal price, int stock, string category, string? description, bool? isActive, DateTime? deactivatedAtUtc)
  {
    var validated = new Product(ProductCode, name, price, stock, category, description, isActive, deactivatedAtUtc);

    Name = validated.Name;
    Price = validated.Price;
    Stock = validated.Stock;
    Category = validated.Category;
    Description = validated.Description;
    IsActive = validated.IsActive;
    DeactivatedAtUtc = validated.DeactivatedAtUtc;
  }

  public void AssignCategory(Category category)
  {
    ArgumentNullException.ThrowIfNull(category);
    if (category.CategoryId <= 0)
      throw new InvalidOperationException("Simpan Category terlebih dahulu agar dapat CategoryId");

    CategoryId = category.CategoryId;
    CategoryEntity = category;
    Category = category.Name;
  }
}