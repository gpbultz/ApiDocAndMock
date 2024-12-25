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
            services.TryAddSingleton<IMockConfigurationsFactory, MockConfigurationsFactory>();

            services.Configure<MockingConfigurationOptions>(options =>
            {
                options.Configure = configure;
            });

            services.AddSingleton<IHostedService, ApplyConfigurationsHostedService>();

            return services;
        }

        public static IServiceCollection AddDefaultFakerRules(this IServiceCollection services, Action<Dictionary<string, Func<Faker, object>>> configure)
        {
            services.TryAddSingleton<IMockConfigurationsFactory, MockConfigurationsFactory>();

            services.Configure<FakerRuleOptions>(options =>
            {
                options.Configure = configure;
            });

            services.AddSingleton<IHostedService, ApplyConfigurationsHostedService>();

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

    }
}
