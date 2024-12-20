using ApiDocAndMock.Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Mocking;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMockingConfigurations(this IServiceCollection services, Action<MockingConfigurations> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions), "Configuration options must be provided.");
            }

            var configurations = new MockingConfigurations();
            configureOptions(configurations);

            services.AddSingleton(configurations);
            services.AddSingleton<IApiMockDataFactory>(new ApiMockDataFactory(configurations));

            return services;
        }

        public static IServiceCollection AddCommonResponseConfigurations(this IServiceCollection services, Action<CommonResponseConfigurations>? configureOptions = null)
        {
            var configurations = new CommonResponseConfigurations();
            configureOptions?.Invoke(configurations);

            services.AddSingleton(configurations);
            return services;
        }
    }
}
