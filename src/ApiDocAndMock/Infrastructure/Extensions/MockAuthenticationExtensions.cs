using ApiDocAndMock.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

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
        public static IServiceCollection AddMockAuthentication(this IServiceCollection services, Action<List<string>>? roles = null, Action<JwtBearerOptions>? configureJwt = null)
        {
            // Set up basic Bearer authentication
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = false,
                    };

                    configureJwt?.Invoke(options);
                });

            // Configure Authorization and add roles if provided
            services.AddAuthorization(options =>
            {
                var roleList = new List<string>();

                // Invoke action to populate roles dynamically
                roles?.Invoke(roleList);

                foreach (var role in roleList)
                {
                    options.AddPolicy($"{role}Only", policy =>
                    {
                        policy.RequireRole(role);
                    });
                }
            });

            return services;
        }


    }
}
