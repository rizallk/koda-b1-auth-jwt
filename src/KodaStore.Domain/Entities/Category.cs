namespace KodaStore.Domain.Entities;

public sealed class Category
{
  private Category() { }

  public int CategoryId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public ICollection<Product> Products { get; private set; } = new List<Product>();

  public Category(string name)
  {
    if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 30)
      throw new ArgumentException("Nama kategori wajib diisi, maksimal 30 karakter.", nameof(name));

    Name = name.Trim();
  }
}