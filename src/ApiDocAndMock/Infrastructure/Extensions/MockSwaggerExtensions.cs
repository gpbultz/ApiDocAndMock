using ApiDocAndMock.Infrastructure.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    /// <summary>
    /// Service collection extension for Swagger, Mock Authorization security, and whether to use Annotations for describing schemas on Swagger documentation
    /// </summary>
    public static class MockSwaggerExtensions
    {
        public static IServiceCollection AddMockSwagger(this IServiceCollection services, string title = "Mock API", string version = "v1", bool includeSecurity = false, bool includAnnotations = false)
        {
            services.AddSwaggerGen(options =>
            {
                // Standard Swagger setup
                options.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = version });

                if (includeSecurity)
                {
                    // Add security definition
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Description = "Enter 'mock-token' to authenticate. Optionally add roles via 'X-Roles: Admin,User'.",
                        Scheme = "Bearer"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                }

                if (includAnnotations)
                {
                    options.EnableAnnotations();
                }

                options.OperationFilter<RoleHeaderOperationFilter>();
            });

            return services;
        }
    }
}
