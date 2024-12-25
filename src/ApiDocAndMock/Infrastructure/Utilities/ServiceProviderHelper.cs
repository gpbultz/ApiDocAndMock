using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static IHttpContextAccessor? ResolveHttpContextAccessor()
        {
            return _serviceProvider?.GetService<IHttpContextAccessor>();
        }

        public static IServiceProvider ResolveServiceProvider()
        {
            var httpContextAccessor = ResolveHttpContextAccessor();

            var serviceProvider = httpContextAccessor?.HttpContext?.RequestServices
                                  ?? _serviceProvider;

            if (serviceProvider == null)
            {
                throw new InvalidOperationException("Unable to resolve IServiceProvider from HttpContext or fallback provider.");
            }

            return serviceProvider;
        }
    }

}
