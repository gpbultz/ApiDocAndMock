## Overview

`CreateMockWithMemoryDb` is an extension method for `RouteHandlerBuilder` that simplifies the creation of endpoint filters that simulate data persistence using an in-memory database. This method is particularly useful for testing or mocking API endpoints by storing incoming request data as new objects in an in-memory store.

## Method Signature

```csharp
public static RouteHandlerBuilder CreateMockWithMemoryDb<TRequest, TStored, TResponse>(
    this RouteHandlerBuilder builder,
    Func<TRequest, TStored>? customMapper = null,
    string idFieldName = "Id",
    Func<object>? generateId = null,
    Func<TStored, string>? locationPathBuilder = null)
    where TRequest : class
    where TStored : class, new()
    where TResponse : class, new()
```

### Type Parameters

- **`TRequest`**: The type of the incoming request object.
- **`TStored`**: The type of the object to be stored in the in-memory database. This type must have a parameterless constructor.
- **`TResponse`**: The type of the response object returned by the endpoint.

## Parameters

- **`builder`** _(RouteHandlerBuilder)_ – The route handler builder to which the filter is applied.
- **`customMapper`** _(Func<TRequest, TStored>?, optional)_ – A function that maps the incoming request to the stored object. If `null`, a default mapping method is used.
- **`idFieldName`** _(string, optional)_ – The name of the property in `TStored` and `TResponse` representing the object's unique identifier. Defaults to "Id".
- **`generateId`** _(Func?, optional)_ – A function to generate a unique identifier for the stored object. If `null`, a new GUID is used.
- **`locationPathBuilder`** _(Func<TStored, string>?, optional)_ – A function to build the location path for the created object. If `null`, a default path is constructed using the type name of `TStored`.

## Return Value

Returns a `RouteHandlerBuilder` with the endpoint filter applied.

## Usage

### Basic Example

```csharp
app.MapPost("/api/mock", (CreateRequest request) => Results.Ok())
    .CreateMockWithMemoryDb<CreateRequest, StoredEntity, ResponseDto>();
```

In this example:

- `CreateRequest` is the incoming API request.
- `StoredEntity` is the entity stored in the memory database.
- `ResponseDto` is the response returned to the client.

### Custom Mapper and ID Generation

```csharp
app.MapPost("/api/custom", (CustomRequest request) => Results.Ok())
    .CreateMockWithMemoryDb<CustomRequest, CustomEntity, CustomResponse>(
        customMapper: req => new CustomEntity { Name = req.Name },
        generateId: () => DateTime.Now.Ticks
    );
```

This example demonstrates how to provide a custom mapping function and ID generator.

### Location Path Override

```csharp
app.MapPost("/api/entity", (EntityRequest request) => Results.Ok())
    .CreateMockWithMemoryDb<EntityRequest, Entity, EntityResponse>(
        locationPathBuilder: entity => $"/api/entity/{entity.Id}"
    );
```

Here, the location path for the created object is explicitly defined.

## Internal Logic

1. **Dependency Injection** – Retrieves `IMemoryDb` from the DI container.
2. **Mapping** – Uses either the provided `customMapper` or a default method to map the request to the stored object.
3. **ID Assignment** – Generates a unique ID using `generateId` or defaults to a GUID.
4. **Persistence** – Adds the new object to the in-memory database.
5. **Response Creation** – A response object is created and populated with the generated ID.
6. **Result Return** – The method returns a `201 Created` response if `locationPathBuilder` is provided; otherwise, an `200 OK` response is returned.

## Dependencies

- **`IMemoryDb`** – This method relies on an in-memory database service that must be registered in the dependency injection container.
- **`ApiMockDataFactory`** – A helper class for creating mock data during the mapping process.

## Notes

- Ensure `TStored` and `TResponse` have a writable property named `Id` (or the specified `idFieldName`).
- `IMemoryDb` should implement methods like `Add` to persist objects.

---

This documentation provides a comprehensive overview of the `CreateMockWithMemoryDb` method, outlining its use cases, customization options, and dependencies for seamless integration into your project.