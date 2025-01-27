﻿using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Data;
using ApiDocAndMock.Infrastructure.Handlers;
using ApiDocAndMock.Shared.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class MemoryDbExtensions
    {
        /// <summary>
        /// Inject the MemoryDb into the Program as a singleton
        /// </summary>
        public static IServiceCollection AddMemoryDb(this IServiceCollection services)
        {
            services.AddSingleton<IMemoryDbHandler, MemoryDbHandler>();
            services.AddSingleton<IMemoryDb>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var logger = sp.GetRequiredService<ILogger<NoOpMemoryDb>>();

                if (env.IsProduction())
                {
                    return new NoOpMemoryDb(logger);
                }

                return new MemoryDb();
            });


            return services;
        }

        /// <summary>
        /// Create a new record in the MemoryDb. All parameters are optional
        /// </summary>
        /// <typeparam name="TRequest">The request object that will be containing the values to save</typeparam>
        /// <typeparam name="TStored">The object that will be saved to the database</typeparam>
        /// <typeparam name="TResponse">The response confirming the success of the save operation</typeparam>
        /// <param name="customMapper">Build some custom mapping if you want to map values from TRequest to TStored</param>
        /// <param name="idFieldName">Name of the id field for TRequest and TResponse. Defaults to "Id" if nothing provided</param>
        /// <param name="generateId">Generate a unique identifier, for example a Random int. Leave null to generate a random Guid. Will apply to this data type for the Id field on TResponse</param>
        /// <param name="locationPathBuilder">Path that will be included in response header to indicate where resource can be accessed via the api. Defaults to /{typeof(TStored).Name.ToLower()}s/{newId} if left null</param>
        /// <returns>A Result.Created with locationPath, and a populated instance of TResponse</returns>
        public static RouteHandlerBuilder CreateMockWithMemoryDb<TRequest, TStored, TResponse>(this RouteHandlerBuilder builder, Func<TRequest, TStored>? customMapper = null, string idFieldName = "Id", Func<object>? generateId = null, Func<TStored, string>? locationPathBuilder = null)
            where TRequest : class
            where TStored : class, new()
            where TResponse : class, new()
        {
            return builder.AddEndpointFilter(async (context, next) =>
            {
                var mockDataFactory = context.HttpContext.RequestServices.GetRequiredService<IApiMockDataFactory>();
                var request = context.GetArgument<TRequest>(0);
                var handler = context.HttpContext.RequestServices.GetRequiredService<IMemoryDbHandler>();
                var stored = mockDataFactory.CreateMockObject<TStored>();

                var (response, locationPath) = handler.CreateMockWithMemoryDb<TRequest, TStored, TResponse>(request, stored, customMapper, idFieldName, generateId, locationPathBuilder);
                return Results.Created(locationPath, response);

            });
        }

        /// <summary>
        /// Update an existing record in MemoryDb. All parameters are optional
        /// </summary>
        /// <typeparam name="TRequest">Type of request object that will be read from Context body in order to obtain data to save</typeparam>
        /// <typeparam name="TStored">Type of object to be looked up and saved to database</typeparam>
        /// <typeparam name="TResponse">Type of object for the endpoint response</typeparam>
        /// <param name="sourceIdFieldName">Name of the id field on TRequest. Default value of "Id"</param>
        /// <param name="queryIdFieldName">Name of the field to query the database by in order to obtain existing record. Default of "Id"</param>
        /// <param name="customMapper">Define any custom mapping from TRequest to TStored, otherwise same field names will be populated from TRequest to TStored</param>
        /// <param name="responseMapper">Define any value mapping for TResponse, otherwise a TResponse will be mocked</param>
        /// <param name="defaultMethodOutcome">Set by querystring parameter methodOutcome to simulate a 200, 201 or 204 response</param>
        /// <param name="locationPath">Path that updated resource would be accessible from to be written to response header</param>
        public static RouteHandlerBuilder UpdateMockWithMemoryDb<TRequest, TStored, TResponse>(this RouteHandlerBuilder builder, string sourceIdFieldName = "Id", string queryIdFieldName = "Id",
                        Func<TRequest, TStored>? customMapper = null, Func<TStored, TResponse>? responseMapper = null, string? defaultMethodOutcome = "Return200", string? locationPath = "")
            where TRequest : class
            where TStored : class, new()
            where TResponse : class, new()
        {
            return builder.AddEndpointFilter(async (context, next) =>
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<IMemoryDb>();
                var mockFactory = context.HttpContext.RequestServices.GetRequiredService<IApiMockDataFactory>();
                var handler = context.HttpContext.RequestServices.GetRequiredService<IMemoryDbHandler>();

                var request = context.GetArgument<TRequest>(0);
                var id = context.HttpContext.Request.RouteValues[sourceIdFieldName];

                if (id == null)
                {
                    var sourceIdProperty = request.GetType().GetProperty(sourceIdFieldName);
                    if (sourceIdProperty == null)
                    {
                        throw new ArgumentException($"Field {sourceIdFieldName} does not exist on type {typeof(TRequest).Name}");
                    }

                    id = sourceIdProperty.GetValue(request);

                    if (id == null)
                    {
                        return Results.BadRequest($"The {sourceIdFieldName} field cannot be null.");
                    }
                }

                // Retrieve or create a mock of the existing object
                var updatedObject = db.GetByField<TStored>(queryIdFieldName, id) ?? mockFactory.CreateMockObject<TStored>() ?? new TStored();

                var (response, outcome) = handler.UpdateMockWithMemoryDb(request, id, updatedObject, sourceIdFieldName, queryIdFieldName, customMapper, responseMapper, defaultMethodOutcome, locationPath);

                return outcome switch
                {
                    "Return200" => Results.Ok(response),
                    "Return201" => Results.Created(locationPath ?? "", response),
                    "Return204" => Results.NoContent(),
                    _ => throw new InvalidOperationException($"Unsupported method outcome: {outcome}")
                };
            })
            .WithOpenApi(operation =>
            {
                // Add OpenAPI documentation for methodOutcome parameter
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "methodOutcome",
                    In = ParameterLocation.Query,
                    Description = "Specify the response behavior (Return200, Return201, or Return204). Overrides default behavior.",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = new List<IOpenApiAny>
                        {
                            new OpenApiString("Return200"),
                            new OpenApiString("Return201"),
                            new OpenApiString("Return204")
                        }
                    }
                });

                if (!operation.Parameters.Any(p => string.Equals(p.Name, sourceIdFieldName, StringComparison.OrdinalIgnoreCase) && p.In == ParameterLocation.Path))
                {
                    var idFieldProperty = typeof(TRequest).GetProperty(sourceIdFieldName);
                    var parameterLocation = idFieldProperty != null ? ParameterLocation.Query : ParameterLocation.Path;

                    var schemaType = idFieldProperty?.PropertyType == typeof(Guid) ? "string" : idFieldProperty?.PropertyType.Name.ToLower();
                    var schemaFormat = idFieldProperty?.PropertyType == typeof(Guid) ? "uuid" : null;

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = sourceIdFieldName,
                        In = parameterLocation,
                        Description = $"The unique identifier for the object being updated (from {parameterLocation}).",
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = schemaType ?? "string",
                            Format = schemaFormat
                        }
                    });
                }

                return operation;
            });
        }

        /// <summary>
        /// Delete a record from the database. All parameters are optional
        /// </summary>
        /// <typeparam name="TStored">Type of object to be looked up in database</typeparam>
        /// <typeparam name="TResponse">Type of object that will be used for the response to the endpoint</typeparam>
        /// <param name="idFieldName">Name of field to look up database object. Defaults to "Id"</param>
        /// <param name="responseMapper">Value for a response to be returned, otherwise a mocked response will be set</param>
        /// <param name="defaultMethodBehaviour">Depending on value of methodOutcome, will set an outcome result of 200, 201 or 204. Defaults to 204</param>
        public static RouteHandlerBuilder DeleteMockWithMemoryDb<TStored, TResponse>(this RouteHandlerBuilder builder, string idFieldName = "Id", Func<TStored, TResponse>? responseMapper = null, DefaultMethodBehaviour defaultMethodBehaviour = DefaultMethodBehaviour.Return204)
            where TStored : class, new()
            where TResponse : class, new()
        {
            return builder.AddEndpointFilter(async (context, next) =>
            {
                var handler = context.HttpContext.RequestServices.GetRequiredService<IMemoryDbHandler>();

                var id = context.GetArgument<object>(0);
                var query = context.HttpContext.Request.Query;

                var behaviour = defaultMethodBehaviour;
                if (query.TryGetValue("methodOutcome", out var outcome))
                {
                    behaviour = outcome.ToString() switch
                    {
                        "Return200" => DefaultMethodBehaviour.Return200,
                        "Return201" => DefaultMethodBehaviour.Return201,
                        "Return204" => DefaultMethodBehaviour.Return204,
                        _ => defaultMethodBehaviour
                    };
                }

                var (response, methodBehaviour) = handler.DeleteMockWithMemoryDb<TStored, TResponse>(id, idFieldName, responseMapper, behaviour);

                return methodBehaviour switch
                {
                    DefaultMethodBehaviour.Return200 => Results.Ok(response),
                    DefaultMethodBehaviour.Return201 => Results.Created($"/{typeof(TStored).Name.ToLower()}s/{id}", response),
                    DefaultMethodBehaviour.Return204 => Results.NoContent(),
                    _ => throw new InvalidOperationException($"Unsupported method outcome: {methodBehaviour}")
                };
            })
            .WithOpenApi(operation =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "methodOutcome",
                    In = ParameterLocation.Query,
                    Description = "Specify the response behavior (Return200, Return201, or Return204). Overrides default behavior.",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = new List<IOpenApiAny>
                        {
                            new OpenApiString("Return200"),
                            new OpenApiString("Return201"),
                            new OpenApiString("Return204")
                        }
                    }
                });

                if (!operation.Parameters.Any(p => p.Name.ToLower() == idFieldName.ToLower() && p.In == ParameterLocation.Path))
                {
                    var idFieldProperty = typeof(TStored).GetProperty(idFieldName);

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = idFieldName,
                        In = ParameterLocation.Path,
                        Description = "The unique identifier for the object being deleted.",
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = idFieldProperty?.PropertyType == typeof(Guid) ? "string" : idFieldProperty?.PropertyType.Name.ToLower()
                        }
                    });
                }


                return operation;
            });
        }


        /// <summary>
        /// Get an object from the Database. All properties are optional
        /// </summary>
        /// <typeparam name="T">Type of object to return from database</typeparam>
        /// <param name="idFieldName">Field used to query database. Defaults to "Id"</param>
        /// <param name="defaultBehaviour">Depending on value of querystring methodOutcome, can simulate a mocked record if not found (ReturnMockIfNotFound) or return a 404 (Return404)</param>
        public static RouteHandlerBuilder GetMockFromMemoryDb<T>(this RouteHandlerBuilder builder, string idFieldName = "Id", NotFoundBehaviour defaultBehaviour = NotFoundBehaviour.Return404) where T : class, new()
        {
            return builder
                .WithOpenApi(operation =>
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "methodOutcome",
                        In = ParameterLocation.Query,
                        Description = "Specify the behavior when the object is not found (ReturnMockIfNotFound or Return404). Overrides default behavior.",
                        Required = false,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("ReturnMockIfNotFound"),
                                new OpenApiString("Return404")
                            }
                        }
                    });
                    return operation;
                })
                .AddEndpointFilter(async (context, next) =>
                {
                    var handler = context.HttpContext.RequestServices.GetRequiredService<IMemoryDbHandler>();
                    var mockFactory = context.HttpContext.RequestServices.GetRequiredService<IApiMockDataFactory>();
                    var query = context.HttpContext.Request.Query;

                    var id = context.GetArgument<object>(0);

                    var behaviour = defaultBehaviour;
                    if (query.TryGetValue("methodOutcome", out var outcome))
                    {
                        behaviour = outcome.ToString() switch
                        {
                            "ReturnMockIfNotFound" => NotFoundBehaviour.ReturnMockIfNotFound,
                            "Return404" => NotFoundBehaviour.Return404,
                            _ => defaultBehaviour
                        };
                    }

                    T mockedItem = null;
                    if (behaviour == NotFoundBehaviour.ReturnMockIfNotFound)
                    {
                        mockedItem = mockFactory.CreateMockObject<T>();
                        typeof(T).GetProperty(idFieldName)?.SetValue(mockedItem, id);
                    }

                    var (item, resultBehaviour) = handler.GetMockFromMemoryDb<T>(id, idFieldName, behaviour, mockedItem);

                    return item != null ? Results.Ok(item) : Results.NotFound();
                });
        }
    }
}
