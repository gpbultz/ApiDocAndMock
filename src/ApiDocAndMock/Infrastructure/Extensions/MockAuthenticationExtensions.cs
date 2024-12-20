using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
        public static IServiceCollection AddMockAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, // Skip issuer validation
                        ValidateAudience = false, // Skip audience validation
                        ValidateLifetime = false, // Skip token expiration validation
                        ValidateIssuerSigningKey = false, // Skip signing key validation
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("Token validated successfully.");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("Token validation failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// Adds authentication with configured JwtBearerOptions
        /// </summary>
        /// <param name="configure">Options to apply</param>
        /// <returns></returns>
        public static IServiceCollection AddMockAuthentication(this IServiceCollection services, Action<JwtBearerOptions>? configure = null)
        {
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

                    configure?.Invoke(options);
                });

            services.AddAuthorization();

            return services;
        }

    }
}
