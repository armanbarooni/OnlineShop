using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application;
using FluentValidation;
using MediatR;
using OnlineShop.Application.Common.Behaviors;
using AutoMapper;

namespace OnlineShop.Application.Common
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly)
            );

            // Manually register AutoMapper to avoid extension method ambiguity and package dependency
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(AssemblyReference).Assembly);
            });
            services.AddSingleton<IMapper>(mapperConfig.CreateMapper());

            // Register FluentValidation validators from this assembly
            services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);

            // Add validation pipeline behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
