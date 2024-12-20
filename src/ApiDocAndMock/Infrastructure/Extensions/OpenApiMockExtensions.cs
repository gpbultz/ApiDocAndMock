using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Mocking;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class OpenApiMockExtensions
    {
        /// <summary>
        /// Documents a Mock Request for an endpoint
        /// </summary>
        /// <typeparam name="T">Type of object to mock</typeparam>
        /// <param name="builder">Extension of RouteHandlerBuilder</param>
        /// <returns>A mocked object that will be displayed by Swagger in the Example box</returns>
        public static RouteHandlerBuilder WithMockRequest<T>(this RouteHandlerBuilder builder) where T : class, new()
        {
            return builder.WithOpenApi(operation =>
            {
                // Generate the mock example using the static factory
                var mockExample = ApiMockDataFactory.CreateMockObjects<T>(1).First();

                // Set the request body in the OpenAPI operation
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new Microsoft.OpenApi.Any.OpenApiString(JsonSerializer.Serialize(mockExample))
                        }
                    }
                };

                return operation;
            });
        }


        /// <summary>
        /// Documents a Mock Response for an endpoint
        /// </summary>
        /// <typeparam name="T">Type of object to mock</typeparam>
        /// <param name="builder">Extension of RouteHandlerBuilder</param>
        /// <returns>A mocked object that will be displayed by Swagger in the Example box</returns>
        public static RouteHandlerBuilder WithMockResponse<T>(this RouteHandlerBuilder builder) where T : class, new()
        {
            return builder.WithOpenApi(operation =>
            {                
                // Generate mock data
                var mockExample = ApiMockDataFactory.CreateMockObjects<T>(count: 1).First();

                // Define the response for Swagger
                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "Successful response",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new Microsoft.OpenApi.Any.OpenApiString(JsonSerializer.Serialize(mockExample)),
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = typeof(T).Name
                                }
                            }
                        }
                    }
                };

                return operation;
            })
            .Produces<T>(200); // Register schema globally
        }


        /// <summary>
        /// Documents a list of Mocked objects for a Response for an endpoint
        /// </summary>
        /// <typeparam name="T">Type of object to mock</typeparam>
        /// <param name="builder">Extension of RouteHandlerBuilder</param>
        /// <param name="count">Number of objects to mock</param>
        /// <returns>A list of mocked objects that will be displayed by Swagger in the Example box</returns>
        public static RouteHandlerBuilder WithMockResponseList<T>(this RouteHandlerBuilder builder, int count = 5) where T : class, new()
        {
            return builder.WithOpenApi(operation =>
            {
                // Generate the mock examples using the static factory
                var mockExamples = ApiMockDataFactory.CreateMockObjects<T>(count);

                // Set the response in the OpenAPI operation
                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "Successful response with a list of mocked objects",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new Microsoft.OpenApi.Any.OpenApiString(JsonSerializer.Serialize(mockExamples)),
                            Schema = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.Schema,
                                        Id = typeof(T).Name
                                    }
                                }
                            }
                        }
                    }
                };

                return operation;
            });
        }

    }


}
