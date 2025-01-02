using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Authorization;
using ApiDocAndMock.Infrastructure.Utilities;
using ApiDocAndMock.Shared.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class OpenApiExtensions
    {
        /// <summary>
        /// Enables authorization on endpoint, and documents that endpoint will require Bearer token
        /// </summary>
        public static RouteHandlerBuilder RequireBearerToken(this RouteHandlerBuilder builder)
        {
            builder.RequireAuthorization();

            return builder.WithOpenApi(operation =>
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            }
                        ] = Array.Empty<string>()
                    }
                };

                return operation;
            });
        }

        public static RouteHandlerBuilder WithAuthorizationRoles(this RouteHandlerBuilder builder, params string[] roles)
        {
            var settings = ServiceProviderHelper.GetService<IOptions<AuthSettings>>().Value;

            if (roles.Length == 0)
            {
                return builder;
            }

            if (settings.Mode == AuthMode.JWTToken)
            {
                builder.RequireAuthorization(policy =>
                {
                    policy.RequireRole(roles);
                });
            }
            else if (settings.Mode == AuthMode.XRolesHeader)
            {
                builder.RequireAuthorization(policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        var userRoles = context.User.Claims
                                        .Where(c => c.Type == ClaimTypes.Role)
                                        .Select(c => c.Value);
                        return roles.Any(role => userRoles.Contains(role));
                    });
                });
            }

            return builder.WithOpenApi(operation =>
            {
                operation.Parameters ??= new List<OpenApiParameter>();

                if (settings.Mode == AuthMode.XRolesHeader &&
                    !operation.Parameters.Any(p => p.Name.Equals("X-Roles", StringComparison.OrdinalIgnoreCase)))
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "X-Roles",
                        In = ParameterLocation.Header,
                        Required = true,
                        Description = $"Roles required: {string.Join(", ", roles)}",
                        Schema = new OpenApiSchema { Type = "string" }
                    });
                }

                return operation;
            });
        }



        /// <summary>
        /// Documents any querystring parameters that are required
        /// </summary>
        public static RouteHandlerBuilder WithRequiredQueryParameter(this RouteHandlerBuilder builder, string parameterName, string description = "")
        {
            return builder.WithOpenApi(operation =>
            {
                if (operation.Parameters == null) return operation;

                foreach (var parameter in operation.Parameters)
                {
                    if (parameter.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        parameter.Required = true;
                        parameter.Description = !string.IsNullOrEmpty(description)
                            ? description
                            : $"{parameterName} is required.";
                    }
                }

                return operation;
            });
        }

        /// <summary>
        /// Documents any value that resides in the address, e.g. /Contacts/{id}
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="description">Description of the parameter for Swagger Documentation</param>
        /// <param name="type">Data type for the parameter</param>
        /// <param name="format">Format of the parameter expected e.g. uuid</param>
        public static RouteHandlerBuilder WithPathParameter(this RouteHandlerBuilder builder, string name, string description, string type = "string", string format = "uuid")
        {
            return builder.WithOpenApi(operation =>
            {
                operation.Parameters ??= new List<OpenApiParameter>();

                var existingParameter = operation.Parameters.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());

                if (existingParameter != null)
                {
                    existingParameter.Description = description;
                    existingParameter.Schema ??= new OpenApiSchema();
                    existingParameter.Schema.Type = type;
                    existingParameter.Schema.Format = format;
                }
                else
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = name,
                        In = ParameterLocation.Path,
                        Description = description,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = type,
                            Format = format
                        }
                    });
                }

                return operation;
            });
        }


        /// <summary>
        /// Add a summary for the endpoint to the Swagger documentation
        /// </summary>
        /// <param name="summary">Title for the summary</param>
        /// <param name="description">Description in more detail on what the endpoint will do for Swagger Documentation</param>
        /// <returns></returns>
        public static RouteHandlerBuilder WithSummary(this RouteHandlerBuilder builder, string summary, string description)
        {
            return builder.WithOpenApi(operation =>
            {
                operation.Summary = summary;
                operation.Description = description;
                return operation;
            });
        }

        /// <summary>
        /// Sets a static request Object without mocked values for Swagger documentation
        /// </summary>
        /// <typeparam name="T">Type of object to document</typeparam>
        /// <param name="example">An instance of the object with pre-populated values</param>
        public static RouteHandlerBuilder WithStaticRequest<T>(this RouteHandlerBuilder builder, T example) where T : class
        {
            return builder.WithOpenApi(operation =>
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiString(JsonSerializer.Serialize(example))
                        }
                    }
                };
                return operation;
            });
        }

        /// <summary>
        /// Sets a static response Object without mocked values for Swagger documentation
        /// </summary>
        /// <typeparam name="T">Type of object to document</typeparam>
        /// <param name="statusCode">The HTTP status code that will be set with this object</param>
        /// <param name="example">An instance of the object with pre-populated values</param>
        public static RouteHandlerBuilder WithStaticResponse<T>(this RouteHandlerBuilder builder, string statusCode, T example) where T : class
        {
            return builder.WithOpenApi(operation =>
            {
                operation.Responses[statusCode] = new OpenApiResponse
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiString(JsonSerializer.Serialize(example))
                        }
                    }
                };
                return operation;
            });
        }

        public static RouteHandlerBuilder WithValidationErrors<T>(this RouteHandlerBuilder builder) where T : class, new()
        {
            var example400Error = GenerateMalformedRequestError<T>();
            var example422Error = GenerateValidationError<T>();

            return builder.WithOpenApi(operation =>
            {
                // Document 400 for malformed requests
                operation.Responses["400"] = new OpenApiResponse
                {
                    Description = "Bad Request - Malformed request syntax or invalid JSON.",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiString(JsonSerializer.Serialize(example400Error))
                        }
                    }
                };

                // Document 422 for validation errors
                operation.Responses["422"] = new OpenApiResponse
                {
                    Description = "Unprocessable Entity - Validation errors occurred.",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiString(JsonSerializer.Serialize(example422Error))
                        }
                    }
                };

                return operation;
            });
        }

        /// <summary>
        /// Provides documentation for mockOutcome querystring parameter that can be applied to the endpoint that will trigger the Mockoutcome Middleware with HTTP response
        /// </summary>
        public static RouteHandlerBuilder WithMockOutcome(this RouteHandlerBuilder builder)
        {
            return builder.WithOpenApi(operation =>
            {
                operation.Parameters ??= new List<OpenApiParameter>();

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "mockOutcome",
                    In = ParameterLocation.Query,
                    Description = "Triggers a specific HTTP status code response. Supported values: 401, 429, 500.",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Format = "int32",
                        Enum = new List<IOpenApiAny>
                    {
                        new OpenApiInteger(401),
                        new OpenApiInteger(429),
                        new OpenApiInteger(500)
                    }
                    }
                });

                return operation;
            });
        }

        /// <summary>
        /// Adds a list of common repsonses to Swagger documentation by status code without having to define responses
        /// </summary>
        /// <param name="statusCodes">Status codes to include on Swagger endpoint documentation</param>
        /// <returns></returns>
        public static RouteHandlerBuilder WithCommonResponses(this RouteHandlerBuilder builder, params string[] statusCodes)
        {
            return builder.WithOpenApi(operation =>
            {
                var mockDataFactory = ServiceProviderHelper.GetService<IMockConfigurationsFactory>();
                var responseConfigurations = ServiceProviderHelper.GetService<ICommonResponseConfigurations>();

                foreach (var statusCode in statusCodes)
                {
                    if (int.TryParse(statusCode, out var parsedStatusCode))
                    {
                        var problemDetails = responseConfigurations.GetProblemDetailsForStatusCode(parsedStatusCode);
                        if (problemDetails != null)
                        {
                            operation.Responses[statusCode] = new OpenApiResponse
                            {
                                Description = problemDetails.Title ?? "Response",
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Example = new OpenApiString(JsonSerializer.Serialize(problemDetails))
                                    }
                                }
                            };
                        }
                    }
                }

                return operation;
            });
        }



        // Generate example for 400 Bad Request
        private static object GenerateMalformedRequestError<T>() where T : class, new()
        {
            return new
            {
                message = "Invalid request format.",
                details = "The JSON syntax is incorrect, or the request body is empty."
            };
        }

        // Generate example for 422 Unprocessable Entity
        private static object GenerateValidationError<T>() where T : class, new()
        {
            var instance = new T();
            var type = typeof(T);

            var validationErrors = new List<string>();

            foreach (var property in type.GetProperties())
            {
                if (Attribute.IsDefined(property, typeof(RequiredAttribute)))
                {
                    validationErrors.Add($"{property.Name} is required.");
                }

                if (Attribute.IsDefined(property, typeof(EmailAddressAttribute)))
                {
                    validationErrors.Add($"{property.Name} must be a valid email address.");
                }

                if (Attribute.IsDefined(property, typeof(PhoneAttribute)))
                {
                    validationErrors.Add($"{property.Name} must be a valid phone number.");
                }
            }

            return new
            {
                errors = validationErrors
            };
        }
    }
}
