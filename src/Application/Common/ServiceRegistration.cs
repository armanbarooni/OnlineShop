using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly)
        );
        services.AddAutoMapper(cfg => { }, typeof(AssemblyReference).Assembly);
        return services;
    }
}
