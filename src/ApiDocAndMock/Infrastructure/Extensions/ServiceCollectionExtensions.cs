using ApiDocAndMock.Infrastructure.Configurations;
using ApiDocAndMock.Infrastructure.Mocking;
using Microsoft.Extensions.DependencyInjection;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMockingConfigurations(this IServiceCollection services, Action<MockConfigurationsFactoryWrapper> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var wrapper = new MockConfigurationsFactoryWrapper();
            configure(wrapper);

            services.AddSingleton(wrapper);

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
