using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiDocAndMock.Infrastructure.Authorization
{
    public class RoleHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();

            if (hasAuthorize)
            {
                // Check if X-Roles header already exists
                if (!operation.Parameters.Any(p => p.Name == "X-Roles"))
                {
                    // Add the X-Roles header to Swagger UI
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "X-Roles",
                        In = ParameterLocation.Header,
                        Description = "Comma-separated list of roles (e.g., Admin, User).",
                        Required = false,
                        Schema = new OpenApiSchema
                        {
                            Type = "string"
                        }
                    });
                }

                // Ensure Bearer Token is required
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }] = Array.Empty<string>()
                    }
                };
            }
        }
    }


}
