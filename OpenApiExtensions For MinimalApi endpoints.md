## Overview

The `OpenApiExtensions` class provides a collection of extension methods to enhance `RouteHandlerBuilder` endpoints with OpenAPI documentation and additional behavior. These methods simplify the process of adding security, query parameters, request/response examples, and validation details to API endpoints, ensuring comprehensive and consistent Swagger documentation.

## Methods

### 1. `RequireBearerToken`

**Description**: Enables authorization for the endpoint by requiring a Bearer token. It also documents the security scheme for Swagger.

**Usage**:

```csharp
app.MapGet("/secure-endpoint", () => Results.Ok())
   .RequireBearerToken();
```

**OpenAPI Effect**:

- Adds Bearer token authentication to the endpoint.
- Updates Swagger documentation to reflect that a Bearer token is required.

---

### 2. `WithRequiredQueryParameter`

**Description**: Marks specific query string parameters as required for the endpoint, updating their description in Swagger documentation.

**Parameters**:

- `parameterName` _(string)_ – The name of the required query parameter.
- `description` _(string, optional)_ – A description of the query parameter.

**Usage**:

```csharp
app.MapGet("/api/items", () => Results.Ok())
   .WithRequiredQueryParameter("categoryId", "The category ID is required to filter items.");
```

**OpenAPI Effect**:

- Marks the specified query parameter as required.
- Updates the Swagger description.

---

### 3. `WithPathParameter`

**Description**: Adds path parameter documentation for endpoints that include parameters in the route (e.g., `/items/{id}`).

**Parameters**:

- `name` _(string)_ – The name of the path parameter.
- `description` _(string)_ – A description of the path parameter.
- `type` _(string, optional)_ – The data type (default: `string`).
- `format` _(string, optional)_ – The format (e.g., `uuid`).

**Usage**:

```csharp
app.MapGet("/api/items/{id}", (Guid id) => Results.Ok())
   .WithPathParameter("id", "The unique identifier of the item.");
```

**OpenAPI Effect**:

- Adds path parameter documentation to Swagger.

---

### 4. `WithSummary`

**Description**: Adds a summary and detailed description to the endpoint in Swagger documentation.

**Parameters**:

- `summary` _(string)_ – The summary of the endpoint.
- `description` _(string)_ – A detailed description of the endpoint's behavior.

**Usage**:

```csharp
app.MapGet("/api/items", () => Results.Ok())
   .WithSummary("Get Items", "Retrieves all items from the catalog.");
```

**OpenAPI Effect**:

- Updates the endpoint's summary and description in Swagger.

---

### 5. `WithStaticRequest<T>`

**Description**: Sets a static request body example for Swagger documentation, representing the expected structure of incoming requests.

**Parameters**:

- `example` _(T)_ – An example object to display in Swagger.

**Usage**:

```csharp
app.MapPost("/api/items", (Item item) => Results.Ok())
   .WithStaticRequest(new Item { Name = "Sample Item", Price = 9.99 });
```

**OpenAPI Effect**:

- Displays the example request body in Swagger.

---

### 6. `WithStaticResponse<T>`

**Description**: Sets a static response body example for a specific HTTP status code in Swagger documentation.

**Parameters**:

- `statusCode` _(string)_ – The HTTP status code.
- `example` _(T)_ – An example response object.

**Usage**:

```csharp
app.MapGet("/api/items/{id}", (Guid id) => Results.Ok())
   .WithStaticResponse("200", new Item { Name = "Sample Item", Price = 9.99 });
```

**OpenAPI Effect**:

- Displays the example response for the specified status code.

---

### 7. `WithValidationErrors<T>`

**Description**: Adds documentation for `400 Bad Request` and `422 Unprocessable Entity` responses, displaying example validation errors.

**Usage**:

```csharp
app.MapPost("/api/items", (Item item) => Results.Ok())
   .WithValidationErrors<Item>();
```

**OpenAPI Effect**:

- Adds `400` and `422` responses to Swagger with example validation errors.

---

### 8. `WithMockOutcome`

**Description**: Adds a `mockOutcome` query parameter to the endpoint, allowing dynamic testing by triggering specific HTTP status codes (e.g., 401, 429, 500).

**Usage**:

```csharp
app.MapGet("/api/items", () => Results.Ok())
   .WithMockOutcome();
```

**OpenAPI Effect**:

- Adds a `mockOutcome` query parameter to Swagger for testing.

---

### 9. `WithCommonResponses`

**Description**: Adds predefined common responses (e.g., 400, 500) to the endpoint without manually defining them.

**Parameters**:

- `statusCodes` _(params string[])_ – List of status codes to add to the endpoint.

**Usage**:

```csharp
app.MapGet("/api/items", () => Results.Ok())
   .WithCommonResponses("400", "500");
```

**OpenAPI Effect**:

- Adds common responses and example error messages to Swagger.

---

## Notes

- These extensions streamline OpenAPI/Swagger documentation for ASP.NET Minimal APIs.
- They promote consistency and reduce the need to manually define Swagger attributes for each endpoint.
- Ensure `IMemoryDb`, `CommonResponseConfigurations`, and `ApiMockDataFactory` are properly registered in the DI container when using these methods.