using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using ApiDocAndMock.Infrastructure.Mocking;
using ApiDocAndMock.Infrastructure.Utilities;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMockingConfigurations(this IServiceCollection services, Action<IMockConfigurationsFactory> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            // Ensure the factory is registered without immediate resolution
            services.TryAddSingleton<IMockConfigurationsFactory, MockConfigurationsFactory>();

            // Register configuration via IOptions
            services.Configure<MockingConfigurationOptions>(options =>
            {
                options.Configure = configure;
            });

            // Apply configuration after the container is built
            services.AddSingleton<IHostedService, ApplyMockConfigurationsHostedService>();


            return services;
        }

        public static IServiceCollection AddCommonResponseConfigurations(this IServiceCollection services, Action<ICommonResponseConfigurations>? configureOptions = null)
        {
            services.AddSingleton<ICommonResponseConfigurations>(provider =>
            {
                var configurations = new CommonResponseConfigurations();
                configureOptions?.Invoke(configurations);
                return configurations;
            });

                return services;
        }

        public static IServiceCollection AddDefaultFakerRules(this IServiceCollection services, Action<Dictionary<string, Func<Faker, object>>> configure)
        {
            var defaultRules = new Dictionary<string, Func<Faker, object>>();
            configure?.Invoke(defaultRules);

            // Defer configuration until after DI container is built
            services.AddSingleton<IHostedService>(provider =>
            {
                return new DefaultFakerRulesInitializer(provider, defaultRules);
            });

            return services;
        }
    }
}
