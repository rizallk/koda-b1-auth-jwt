using KodaStore.Domain.Entities;
using KodaStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<Product> Products => Set<Product>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();
  public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // User
    modelBuilder.Entity<AppUser>(entity =>
    {
      entity.Property(user => user.DisplayName)
              .HasMaxLength(100)
              .IsRequired();

      entity.Property(user => user.CreatedAtUtc)
              .IsRequired();
    });

    // RefreshToken
    modelBuilder.Entity<RefreshToken>(entity =>
    {
      entity.ToTable("RefreshTokens", "dbo");
      entity.HasKey(token => token.Id);
      entity.Property(token => token.Id)
              .ValueGeneratedNever();
      entity.Property(token => token.TokenHash)
              .HasMaxLength(64)
              .IsUnicode(false).IsRequired();
      entity.HasIndex(token => token.TokenHash)
              .IsUnique();
      entity.HasIndex(token => new
      {
        token.UserId,
        token.FamilyId
      });
      entity.HasIndex(token => token.FamilyId);

      entity.HasOne(token => token.User)
              .WithMany()
              .HasForeignKey(token => token.UserId)
              .OnDelete(DeleteBehavior.Cascade);

      entity.Property(token => token.CreatedAtUtc)
              .HasColumnType("datetimeoffset");
      entity.Property(token => token.ExpiresAtUtc)
              .HasColumnType("datetimeoffset");
      entity.Property(token => token.UsedAtUtc)
              .HasColumnType("datetimeoffset");
      entity.Property(token => token.RevokedAtUtc)
              .HasColumnType("datetimeoffset");
    });

    // Category
    modelBuilder.Entity<Category>(entity =>
    {
      entity.ToTable("Category", "dbo");
      entity.HasKey(c => c.CategoryId);
      entity.Property(c => c.CategoryId).ValueGeneratedOnAdd();
      entity.Property(c => c.Name).HasMaxLength(30).IsRequired();
      entity.HasIndex(c => c.Name).IsUnique();
    });

    // Product
    modelBuilder.Entity<Product>(entity =>
    {
      entity.ToTable("Product", "dbo", table =>
          {
            table.HasCheckConstraint("CK_Product_Price", "[Price] >= 0");

            table.HasCheckConstraint("CK_Product_Stock", "[Stock] >= 0");
          });

      entity.HasKey(p => p.ProductCode);

      entity.Property(p => p.ProductCode)
              .HasMaxLength(30)
              .IsRequired()
              .ValueGeneratedNever();

      entity.Property(p => p.Name)
              .HasMaxLength(120)
              .IsRequired();

      entity.Property(p => p.Price)
              .IsRequired()
              .HasPrecision(18, 2);

      entity.Property(p => p.Stock)
              .IsRequired();

      entity.Property(p => p.Category)
              .HasMaxLength(30)
              .IsRequired();

      entity.Property(p => p.Description)
              .HasMaxLength(100)
              .IsRequired(false);

      entity.Property(p => p.IsActive)
                  .IsRequired()
                  .HasDefaultValue(true);

      entity.Property(p => p.DeactivatedAtUtc)
                  .IsRequired(false);

      entity.Property(p => p.CategoryId).IsRequired(false);
      entity.Property(p => p.RowVersion).IsRowVersion();

      entity.HasOne(p => p.CategoryEntity)
        .WithMany(c => c.Products)
        .HasForeignKey(p => p.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

      entity.HasIndex(p => p.CategoryId);

      entity.HasIndex(p => new
      {
        p.Category,
        p.Price,
        p.Name
      }).HasDatabaseName("IX_Product_Category_Price_Name");
    });

    // Order
    modelBuilder.Entity<Order>(entity =>
       {
         entity.ToTable("Orders", "dbo", table =>
          table.HasCheckConstraint("CK_Orders_Status", "[Status] In ('Pending', 'Confirmed')"));
         entity.HasKey(o => o.OrderId);
         entity.Property(o => o.OrderId).ValueGeneratedOnAdd();
         entity.Property(o => o.OrderNumber).HasMaxLength(35).IsRequired();
         entity.Property(o => o.OrderDate).HasColumnType("datetimeoffset").IsRequired();
         entity.Property(o => o.Status).HasMaxLength(20).IsRequired();
         entity.HasIndex(o => o.OrderDate);
       });

    //  Order Items
    modelBuilder.Entity<OrderItem>(entity =>
    {
      entity.ToTable("OrderItem", "dbo", table =>
      {
        table.HasCheckConstraint("CK_OrderItem_Quantity", "[Quantity] > 0");
        table.HasCheckConstraint("CK_OrderItem_UnitPrice", "[UnitPrice] >= 0");
      });
      entity.HasKey(i => i.OrderItemId);
      entity.Property(i => i.OrderItemId).ValueGeneratedOnAdd();
      entity.Property(i => i.ProductCode).HasMaxLength(30).IsRequired();
      entity.Property(i => i.Quantity).IsRequired();
      entity.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
      entity.Property(i => i.Subtotal).HasComputedColumnSql("[Quantity] * [UnitPrice]", stored: true);

      entity.HasOne(i => i.Order)
          .WithMany(o => o.Items)
          .HasForeignKey(i => i.OrderId)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(i => i.Product)
          .WithMany(p => p.OrderItems)
          .HasForeignKey(i => i.ProductCode)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasIndex(i => new { i.OrderId, i.ProductCode }).IsUnique();
      entity.HasIndex(i => i.ProductCode);
    });
  }
}