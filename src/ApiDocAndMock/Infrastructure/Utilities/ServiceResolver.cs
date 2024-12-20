using Microsoft.Extensions.DependencyInjection;

namespace ApiDocAndMock.Infrastructure.Utilities
{
    public static class ServiceResolver
    {
        private static IServiceProvider? _serviceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider not set.");
            }

            return _serviceProvider.GetRequiredService<T>();
        }
    }

}
