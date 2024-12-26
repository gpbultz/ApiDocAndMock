using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using ApiDocAndMock.Infrastructure.Mocking;
using ApiDocAndMock.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ApiDocAndMockExtensions
    {

        public static IServiceCollection AddDocAndMock(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddSingleton<IApiMockDataFactory, ApiMockDataFactory>();
            services.AddSingleton<IMockConfigurationsFactory, MockConfigurationsFactory>();
            services.AddSingleton<ICommonResponseConfigurations, CommonResponseConfigurations>();

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
            ServiceProviderHelper.Initialize(app.ApplicationServices);

            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            if (!env.EnvironmentName.Equals(Environments.Production, StringComparison.OrdinalIgnoreCase))
            {
                if (useAuthentication)
                {
                    app.UseMockAuthentication();
                }

                if (useMockOutcome)
                {
                    app.UseMockOutcome();
                }
            }
            else
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger<IApplicationBuilder>>();
                logger.LogInformation("Skipping ApiDocAndMock middleware in Production environment.");
            }

            return app;
        }
    }
}
