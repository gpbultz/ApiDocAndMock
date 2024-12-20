using ApiDocAndMock.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ApiDocAndMockExtensions
    {
        /// <summary>
        /// Configures API mocking and documentation features for the application.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance for chaining.</returns>
        public static IApplicationBuilder UseApiDocAndMock(this IApplicationBuilder app, bool useAuthentication = false, bool useMockOutcome = false)
        {
            // Set up the global service provider resolver
            ServiceResolver.SetServiceProvider(app.ApplicationServices);

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
