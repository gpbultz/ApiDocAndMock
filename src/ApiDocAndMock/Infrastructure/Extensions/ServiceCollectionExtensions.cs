using ApiDocAndMock.Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Mocking;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMockingConfigurations(this IServiceCollection services, Action configure)
        {
            configure?.Invoke();
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
