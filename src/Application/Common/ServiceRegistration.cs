using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application;

namespace OnlineShop.Application.Common
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly)
            );

            services.AddAutoMapper(typeof(AssemblyReference).Assembly);

            return services;
        }
    }
}
