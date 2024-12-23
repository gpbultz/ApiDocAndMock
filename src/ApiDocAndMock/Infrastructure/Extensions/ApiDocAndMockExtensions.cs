using ApiDocAndMock.Infrastructure.Mocking;
using ApiDocAndMock.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ApiDocAndMockExtensions
    {

        public static IServiceCollection AddDocAndMock(this IServiceCollection services)
        {
            // Register wrapper only if it doesn't exist
            if (!services.Any(sd => sd.ServiceType == typeof(MockConfigurationsFactoryWrapper)))
            {
                services.AddSingleton<MockConfigurationsFactoryWrapper>();
            }
            return services;
        }

        /// <summary>
        /// Configures API mocking and documentation features for the application.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance for chaining.</returns>
        public static IApplicationBuilder UseApiDocAndMock(this IApplicationBuilder app, bool useAuthentication = false, bool useMockOutcome = false)
        {
            // Set up the global service provider resolver
            ServiceResolver.SetServiceProvider(app.ApplicationServices);
            ApiMockDataFactoryStatic.Initialize(app.ApplicationServices);

            if (useAuthentication)
            {
                // Add authentication mocking middleware
                app.UseMockAuthentication();
            }

            if (useMockOutcome)
            {
                // Add the middleware for mock outcomes (e.g., simulate HTTP status codes)
                app.UseMockOutcome();
            }

            return app;
        }
    }
}
