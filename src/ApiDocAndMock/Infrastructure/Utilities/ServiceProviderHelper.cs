using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ApiDocAndMock.Infrastructure.Utilities
{
    public static class ServiceProviderHelper
    {
        private static IServiceProvider? _serviceProvider;

        public static void Initialize(IServiceProvider provider)
        {
            _serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public static T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider not set.");
            }

            return _serviceProvider.GetRequiredService<T>();
        }

        public static object GetService(Type serviceType)
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider not set.");
            }

            return _serviceProvider.GetRequiredService(serviceType);
        }

        public static IServiceProvider ResolveServiceProvider()
        {
            return _serviceProvider?.GetService<IHttpContextAccessor>()?.HttpContext?.RequestServices
                   ?? _serviceProvider
                   ?? throw new InvalidOperationException("Unable to resolve IServiceProvider from HttpContext or fallback provider.");
        }
    }

}
