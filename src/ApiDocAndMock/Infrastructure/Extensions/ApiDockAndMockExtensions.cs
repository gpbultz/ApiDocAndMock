using ApiDocAndMock.Infrastructure.Middleware;
using ApiDocAndMock.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ApiDockAndMockExtensions
    {
        /// <summary>
        /// Configures API mocking and documentation features for the application.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance for chaining.</returns>
        public static IApplicationBuilder UseApiMockAndDock(this IApplicationBuilder app)
        {
            // Set up the global service provider resolver
            ServiceResolver.SetServiceProvider(app.ApplicationServices);

            // Add authentication mocking middleware
            app.UseMockAuthentication();

            // Add the middleware for mock outcomes (e.g., simulate HTTP status codes)
            app.UseMockOutcome();

            return app;
        }
    }
}
