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
        
        // Product Comparison
        services.AddScoped<IProductComparisonRepository, ProductComparisonRepository>();
        
            // Phase 6 - Coupon & Discount Repositories
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IUserCouponUsageRepository, UserCouponUsageRepository>();

            // Phase 7 - Order Tracking Repositories
            services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
            
            // Phase 8 - Stock Alerts & User Engagement Repositories
            services.AddScoped<IStockAlertRepository, StockAlertRepository>();
        
        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<Domain.Interfaces.Services.IInvoiceService, Infrastructure.Services.InvoiceService>();
        services.AddHttpClient<MahakSyncService>();
        services.AddScoped<MahakSyncService>();
        services.AddHttpClient<MahakOutgoingSyncService>();
        services.AddScoped<MahakOutgoingSyncService>();

        services.AddHttpClient();
        services.AddScoped<Domain.Interfaces.Services.IPaymentGateway, 
            OnlineShop.Infrastructure.PaymentGateways.Sadad.SadadGatewayService>();
        
        // SMS Service Configuration (runtime selection via options)
        services.Configure<SmsSettings>(configuration.GetSection("SmsSettings"));
        services.Configure<OnlineShop.Application.Settings.SmsIrSettings>(configuration.GetSection("SmsIr"));

        services.AddScoped<ISmsService>(sp =>
        {
            var smsIr = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OnlineShop.Application.Settings.SmsIrSettings>>().Value;
            if (!string.IsNullOrWhiteSpace(smsIr.ApiKey))
            {
                return ActivatorUtilities.CreateInstance<SmsIrSmsService>(sp);
            }

            var legacy = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SmsSettings>>().Value;

            // Support legacy provider=SmsIr with ApiKey configured under SmsSettings
            if ((legacy.Provider ?? string.Empty).Equals("smsir", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(legacy.ApiKey))
            {
                var mergedOptions = Microsoft.Extensions.Options.Options.Create(new OnlineShop.Application.Settings.SmsIrSettings
                {
                    ApiKey = legacy.ApiKey,
                    TemplateId = smsIr.TemplateId,      // keep template if provided via SmsIr section
                    UseSandbox = smsIr.UseSandbox,
                    OtpParamName = smsIr.OtpParamName
                });

                return new SmsIrSmsService(mergedOptions, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SmsIrSmsService>>());
            }

            if ((legacy.Provider ?? string.Empty).Equals("kavenegar", StringComparison.OrdinalIgnoreCase))
            {
                // We need HttpClient for KavenegarSmsService; resolve a typed instance
                var httpFactory = sp.GetRequiredService<System.Net.Http.IHttpClientFactory>();
                var httpClient = httpFactory.CreateClient();
                return new KavenegarSmsService(httpClient, sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SmsSettings>>(), sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<KavenegarSmsService>>());
            }

            return ActivatorUtilities.CreateInstance<MockSmsService>(sp);
        });

        return services;
    }
}
