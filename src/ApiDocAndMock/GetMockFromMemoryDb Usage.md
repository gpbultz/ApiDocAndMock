## Overview

`GetMockFromMemoryDb` is an extension method for `RouteHandlerBuilder` that facilitates retrieving objects from an in-memory database via API endpoints. This method provides configurable behavior when the requested object is not found, allowing the endpoint to return either a 404 response or a mocked object.

## Method Signature

```csharp
public static RouteHandlerBuilder GetMockFromMemoryDb<T>(
    this RouteHandlerBuilder builder,
    string idFieldName = "Id",
    NotFoundBehaviour defaultBehaviour = NotFoundBehaviour.Return404)
    where T : class, new()
```

### Type Parameters

- **`T`**: The type of the object to retrieve from the in-memory database. This type must have a parameterless constructor.

## Parameters

- **`builder`** _(RouteHandlerBuilder)_ – The route handler builder to which the filter is applied.
- **`idFieldName`** _(string, optional)_ – The name of the property representing the unique identifier of the object. Defaults to "Id".
- **`defaultBehaviour`** _(NotFoundBehaviour, optional)_ – Specifies the default behavior when the requested object is not found. The options include:
    - `ReturnMockIfNotFound` – Returns a mocked object if the requested object is not found.
    - `Return404` – Returns a `404 Not Found` response (default behavior).

## Return Value

Returns a `RouteHandlerBuilder` with the endpoint filter applied.

## Usage

### Basic Example

```csharp
app.MapGet("/api/mock/{id}", (Guid id) => Results.Ok())
    .GetMockFromMemoryDb<Entity>();
```

In this example:

- `Entity` is the type of object to retrieve from the memory database.

### Overriding Behavior via Query Parameter

```csharp
GET /api/mock/12345?methodOutcome=ReturnMockIfNotFound
```

- `ReturnMockIfNotFound` returns a mocked object if the entity is not found.
- `Return404` returns a `404 Not Found` response.

## Internal Logic

1. **Dependency Injection** – Retrieves `IMemoryDb` from the DI container.
2. **ID Extraction** – Extracts the requested ID from the route or query parameters.
3. **Query Parameter Evaluation** – Checks for the `methodOutcome` query parameter to override the default behavior.
4. **Database Lookup** – Attempts to retrieve the object by the specified ID. If the object is not found:
    - `ReturnMockIfNotFound` generates and returns a mock object with the same ID.
    - `Return404` returns a `404 Not Found` response.
5. **Successful Retrieval** – If the object exists, returns a `200 OK` response with the object.
## Dependencies

- **`IMemoryDb`** – The in-memory database service must be registered in the DI container.
- **`ApiMockDataFactory`** – Used to generate mock objects if the requested entity is not found.

## Notes

- Ensure `T` has a writable property with the name specified by `idFieldName`.
- `IMemoryDb` should implement methods such as `GetByField` to perform entity lookups.
- This method is useful for simulating API responses during development and testing by dynamically returning mock data.
