## Overview

`DeleteMockWithMemoryDb` is an extension method for `RouteHandlerBuilder` that facilitates the deletion of objects from an in-memory database within API endpoints. This method supports configurable response behaviors through query parameters, allowing the endpoint to return different HTTP status codes based on the deletion outcome.

## Method Signature

```csharp
public static RouteHandlerBuilder DeleteMockWithMemoryDb<TStored, TResponse>(
    this RouteHandlerBuilder builder,
    string idFieldName = "Id",
    Func<TStored, TResponse>? responseMapper = null,
    DefaultMethodBehaviour defaultMethodBehaviour = DefaultMethodBehaviour.Return204)
    where TStored : class, new()
    where TResponse : class, new()
```

### Type Parameters

- **`TStored`**: The type of the object to be deleted from the in-memory database. This type must have a parameterless constructor.
- **`TResponse`**: The type of the response object returned by the endpoint after successful deletion.

## Parameters

- **`builder`** _(RouteHandlerBuilder)_ – The route handler builder to which the filter is applied.
- **`idFieldName`** _(string, optional)_ – The name of the property representing the unique identifier of the object to delete. Defaults to "Id".
- **`responseMapper`** _(Func<TStored, TResponse>?, optional)_ – A function that maps the deleted object to a response object. If `null`, a mock response is generated.
- **`defaultMethodBehaviour`** _(DefaultMethodBehaviour, optional)_ – Specifies the default HTTP response behavior. The options include:
    - `Return200` – Return `200 OK` with a response object.
    - `Return201` – Return `201 Created` with a response object.
    - `Return204` – Return `204 No Content` without a response object (default).

## Return Value

Returns a `RouteHandlerBuilder` with the endpoint filter applied.

## Usage

### Basic Example

```csharp
app.MapDelete("/api/mock/{id}", (string id) => Results.NoContent())
    .DeleteMockWithMemoryDb<StoredEntity, ResponseDto>();
```

In this example:

- `StoredEntity` is the entity type to delete from the memory database.
- `ResponseDto` is the response returned after deletion.

### Custom Response Mapper

```csharp
app.MapDelete("/api/custom/{id}", (string id) => Results.NoContent())
    .DeleteMockWithMemoryDb<CustomEntity, CustomResponse>(
        responseMapper: entity => new CustomResponse { Message = $"Deleted {entity.Name}" }
    );
```

This example uses a custom mapper to craft a response message upon successful deletion.

### Specifying Behavior via Query Parameter

```csharp
DELETE /api/entity/123?methodOutcome=Return200
```

- `Return200` returns `200 OK` with a mock response.
- `Return201` returns `201 Created` with a response and location path.
- `Return204` returns `204 No Content` (default behavior).

## Internal Logic

1. **Dependency Injection** – Retrieves `IMemoryDb` from the DI container.
2. **ID Parsing** – Extracts and converts the provided ID to match the type of the `idFieldName` property.
3. **Query Parameter Evaluation** – Checks for the `methodOutcome` query parameter to override the default behavior.
4. **Deletion Logic** – Attempts to delete the entity by matching the ID. If no matching entity is found:
    - `Return204` returns `204 No Content`.
    - `Return200` returns `200 OK` with a mock response.
5. **Response Handling** – Constructs the response based on the outcome:
    - `Return200` returns an OK response with the mapped or mock response.
    - `Return201` returns a Created response with the resource path.
    - `Return204` returns No Content.

## Dependencies

- **`IMemoryDb`** – The in-memory database service must be registered in the DI container.
- **`ApiMockDataFactory`** – Used to generate mock response objects if a response mapper is not provided.

## Notes

- Ensure `TStored` has a property with the name specified in `idFieldName`.
- `IMemoryDb` should implement methods such as `GetByField` and `Delete`.
- The method supports OpenAPI integration by adding `methodOutcome` as a query parameter for flexibility in testing and mocking.
