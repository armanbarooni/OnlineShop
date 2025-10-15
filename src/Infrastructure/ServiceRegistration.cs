using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Infrastructure.Persistence.Repositories;
using OnlineShop.Infrastructure.Services;

namespace OnlineShop.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration."
            );
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Configure Identity
        services.AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        
        // Product Related Repositories
        services.AddScoped<IProductDetailRepository, ProductDetailRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<IProductInventoryRepository, ProductInventoryRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        
        // User Related Repositories
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserAddressRepository, UserAddressRepository>();
        services.AddScoped<IUserPaymentRepository, UserPaymentRepository>();
        services.AddScoped<IUserOrderRepository, UserOrderRepository>();
        services.AddScoped<IUserOrderItemRepository, UserOrderItemRepository>();
        services.AddScoped<IUserReturnRequestRepository, UserReturnRequestRepository>();
        
        // Cart Related Repositories
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<ISavedCartRepository, SavedCartRepository>();
        
        // Mahak Sync Repositories
        services.AddScoped<IMahakSyncLogRepository, MahakSyncLogRepository>();
        services.AddScoped<IMahakMappingRepository, MahakMappingRepository>();
        services.AddScoped<IMahakQueueRepository, MahakQueueRepository>();
        services.AddScoped<ISyncErrorLogRepository, SyncErrorLogRepository>();
        
        // Authentication Repositories
        services.AddScoped<IOtpRepository, OtpRepository>();
        
        // Enhanced Product Repositories
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<ISeasonRepository, SeasonRepository>();
        
        // Phase 5 - Related Products & Recommendations Repositories
        services.AddScoped<IProductRelationRepository, ProductRelationRepository>();
        services.AddScoped<IUserProductViewRepository, UserProductViewRepository>();
        
            // Phase 6 - Coupon & Discount Repositories
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IUserCouponUsageRepository, UserCouponUsageRepository>();

            // Phase 7 - Order Tracking Repositories
            services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
        
        // Services
        services.AddScoped<ITokenService, TokenService>();
        
        // SMS Service Configuration
        var smsSettings = configuration.GetSection("SmsSettings");
        services.Configure<SmsSettings>(smsSettings);
        
        var smsProvider = smsSettings.GetValue<string>("Provider")?.ToLower();
        if (smsProvider == "kavenegar")
        {
            services.AddHttpClient<ISmsService, KavenegarSmsService>();
        }
        else
        {
            // Default to Mock SMS Service for development
            services.AddScoped<ISmsService, MockSmsService>();
        }

        return services;
    }
}
