using KodaStore.Application.Repositories;
using KodaStore.Application.Services;
using KodaStore.Infrastructure.Identity;
using KodaStore.Infrastructure.Persistence;
using KodaStore.Infrastructure.Repositories;
using KodaStore.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KodaStore.Infrastructure;

public static class DepedencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("ecommerce") ?? throw new InvalidOperationException("Connection string ecommerce belum dikonfigurasi");

    services.AddDbContext<AppDbContext>(option =>
    {
      option.UseSqlServer(connectionString)
           .UseSeeding((context, _) =>
           {
             ProductSeedData.Seed(context);
             CategorySeedData.Seed(context);
             OrderSeedData.Seed(context);
           })
           .UseAsyncSeeding(async (context, _, ct) =>
           {
             await ProductSeedData.SeedAsync(context, ct);
             await CategorySeedData.SeedAsync(context, ct);
             await OrderSeedData.SeedAsync(context, ct);
           });
    });

    services.AddScoped<IProductRepository, EfProductRepository>();
    services.AddScoped<IOrderRepository, EfOrderRepository>();
    services.AddScoped<IProductQueryRepository, EfProductQueryRepository>();
    services.AddScoped<ICategoryRepository, EfCategoryRepository>();

    // User
    services.AddIdentityCore<AppUser>(options =>
    {
      options.User.RequireUniqueEmail = true;
      options.Password.RequiredLength = 8;
      options.Password.RequireDigit = true;
      options.Password.RequireLowercase = true;
      options.Password.RequireUppercase = true;
      options.Password.RequireNonAlphanumeric = true;
      options.Password.RequiredUniqueChars = 4;

      options.SignIn.RequireConfirmedEmail = false;
    }).AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();

    services.AddScoped<IAuthService, IdentityAuthService>();

    return services;
  }
}