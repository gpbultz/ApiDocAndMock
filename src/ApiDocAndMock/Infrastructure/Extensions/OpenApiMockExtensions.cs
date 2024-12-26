using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Application.Models.Responses;
using ApiDocAndMock.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text.Json;

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
                var mockDataFactory = ServiceProviderHelper.GetService<IApiMockDataFactory>();

                var mockExample = mockDataFactory.CreateMockObject<T>();

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
                var mockDataFactory = ServiceProviderHelper.GetService<IApiMockDataFactory>();
                var mockExample = mockDataFactory.CreateMockObject<T>();

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
                var mockDataFactory = ServiceProviderHelper.GetService<IApiMockDataFactory>();
                var mockExamples = mockDataFactory.CreateMockObjects<T>(count);

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


        public static RouteHandlerBuilder WithEnrichedMockedResponse<T>(this RouteHandlerBuilder builder, bool includePages = false, bool includeLinks = false, string resourcePath = "", string pageIdField = "Id", int totalCount = 50, int pageSize = 10, int currentPage = 1)
            where T : class, IApiResponse, new()
        {
            return builder.WithOpenApi(operation =>
            {
                var mockDataFactory = ServiceProviderHelper.GetService<IApiMockDataFactory>();

                var mockExample = mockDataFactory.CreateMockObject<T>();

                var id = mockExample.GetType().GetProperty(pageIdField)?.GetValue(mockExample)?.ToString();


                if (includePages)
                {
                    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    mockExample.Pagination = new PaginationMetadata
                    {
                        TotalCount = totalCount,
                        PageSize = pageSize,
                        PageNumber = currentPage,
                        TotalPages = totalPages,
                        First = $"{resourcePath}?page=1",
                        Last = $"{resourcePath}?page={totalPages}",
                        Next = currentPage < totalPages ? $"{resourcePath}?page={currentPage + 1}" : null,
                        Prev = currentPage > 1 ? $"{resourcePath}?page={currentPage - 1}" : null
                    };
                }

                if (includeLinks && !string.IsNullOrEmpty(id))
                {
                    // Replace {id} in the resource path if it exists
                    var resolvedPath = resourcePath.Replace("{id}", id);

                    mockExample.Links = new LinksContainer
                    {
                        Self = resolvedPath,
                        Update = $"{resolvedPath}",
                        Delete = $"{resolvedPath}"
                    };
                }
                else if (includeLinks)
                {
                    // Fallback for endpoints without {id}
                    mockExample.Links = new LinksContainer
                    {
                        Self = resourcePath,
                        Update = $"{resourcePath}/update",
                        Delete = $"{resourcePath}/delete"
                    };
                }

                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "Successful response",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiString(JsonSerializer.Serialize(mockExample))
                        }
                    }
                };
                return operation;
            });
        }
    }
}
