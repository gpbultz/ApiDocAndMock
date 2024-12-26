using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Authorization;
using ApiDocAndMock.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    /// <summary>
    /// Add Mock Authentication to the Service Collection
    /// </summary>
    public static class MockAuthenticationExtensions
    {
        /// <summary>
        /// Adds authentication with predefined JwtBearer options
        /// </summary>
        public static IServiceCollection AddMockAuthentication(this IServiceCollection services, AuthMode authMode = AuthMode.BearerOnly, Action<JwtBearerOptions>? configureJwt = null)
        {
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

            if (env.IsProduction())
            {
                return services; 
            }

            // Store the mode in DI for later access
            var settings = new AuthSettings();
            settings.Mode = authMode;

            services.Configure<AuthSettings>(options =>
            {
                options.Mode = authMode;
            });

            var tokenValidationParams = authMode == AuthMode.JWTToken
                 ? new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidIssuer = "mock-api",
                     ValidateAudience = true,
                     ValidAudience = "mock-clients",
                     ValidateLifetime = true,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("verylongsupersecurekey12345678forHmacSha256")),
                     ValidateIssuerSigningKey = true
                 }
                 : new TokenValidationParameters
                 {
                     ValidateIssuer = false,
                     ValidateAudience = false,
                     ValidateLifetime = false,
                     ValidateIssuerSigningKey = false
                 };

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = tokenValidationParams;
                    configureJwt?.Invoke(options);
                });

            services.AddAuthorization();

            return services;
        }


    }
}
