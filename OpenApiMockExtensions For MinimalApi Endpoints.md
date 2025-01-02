## Overview

The `OpenApiMockExtensions` class provides extension methods for `RouteHandlerBuilder` to enhance API endpoints with mock request and response documentation in OpenAPI (Swagger). These extensions enable the automatic generation of mock data that can be displayed in Swagger's request and response examples, streamlining API documentation and improving developer experience.

## Methods

### 1. `WithMockRequest<T>`

**Description**: Documents a mock request body for an endpoint, generating an example of the specified object type and displaying it in the Swagger request body example box.

**Type Parameter**:

- **`T`**: The type of object to mock. Must have a parameterless constructor.

**Usage**:

```csharp
app.MapPost("/api/items", (Item item) => Results.Ok())
   .WithMockRequest<Item>();
```

**OpenAPI Effect**:

- Generates and displays a mock example of the `Item` class in the Swagger request body.

---

### 2. `WithMockResponse<T>`

**Description**: Documents a mock response for an endpoint, generating an example object of the specified type and displaying it in the Swagger response example box.

**Type Parameter**:

- **`T`**: The type of object to mock. Must have a parameterless constructor.

**Usage**:

```csharp
app.MapGet("/api/items/{id}", (Guid id) => Results.Ok())
   .WithMockResponse<Item>();
```

**OpenAPI Effect**:

- Generates a mock example of `Item` and sets it as the response body for `200 OK`.
- The schema for `Item` is registered globally.

---

### 3. `WithMockResponseList<T>`

**Description**: Documents a mock response list for an endpoint, generating a list of example objects and displaying them in the Swagger response example box.

**Type Parameter**:

- **`T`**: The type of object to mock. Must have a parameterless constructor.

**Parameters**:

- `count` _(int, optional)_ – The number of mock objects to generate (default: `5`).

**Usage**:

```csharp
app.MapGet("/api/items", () => Results.Ok())
   .WithMockResponseList<Item>(count: 10);
```

**OpenAPI Effect**:

- Generates a list of 10 mock `Item` objects.
- Displays the list in the response body for `200 OK`.

---

### 4. `WithEnrichedMockedResponse<T>`

**Description**: Documents a mock response enriched with pagination and hypermedia links, enhancing the realism and usability of API documentation. This method automatically applies pagination and HATEOAS links to the response based on configuration.

**Type Parameter**:

- **`T`**: The type of object to mock. Must inherit from `ApiResponseBase`.

**Parameters**:

- `includePages` _(bool, optional)_ – Whether to include pagination metadata.
- `includeLinks` _(bool, optional)_ – Whether to include HATEOAS links.
- `resourcePath` _(string, optional)_ – The base resource path used to generate links.
- `pageIdField` _(string, optional)_ – The field used to extract the unique identifier (default: `Id`).
- `totalCount` _(int, optional)_ – The total number of records (default: `50`).
- `pageSize` _(int, optional)_ – The number of records per page (default: `10`).
- `currentPage` _(int, optional)_ – The current page number (default: `1`).

**Usage**:

```csharp
app.MapGet("/api/contacts", () => Results.Ok())
   .WithEnrichedMockedResponse<GetContactsResponse>(includePages: true, includeLinks: true, resourcePath: "/api/contacts");
```

**OpenAPI Effect**:

- Generates a mock example enriched with pagination and links.
- Displays pagination metadata if `includePages` is `true`.
- Generates `self`, `update`, and `delete` links if `includeLinks` is `true`.

**Requirements**:

- The response type `T` must implement `IApiResponse` and inherit from `ApiResponseBase`.
- Example:

```csharp
public abstract class ApiResponseBase : IApiResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMetadata? Pagination { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LinksContainer? Links { get; set; }
}

public class GetContactsResponse : ApiResponseBase
{
    public List<GetContactByIdResponse> Contacts { get; set; }
}
```

---

## Internal Logic

### 1. Mock Generation

- Uses `ApiMockDataFactory.CreateMockObjects<T>(count)` to generate mock objects.
- For single responses, the first object from the generated list is used.
- For list responses, the entire list is serialized and returned.

---

## Dependencies

- **`ApiMockDataFactory`** – A static factory responsible for creating mock objects of the specified type.
- **`IMemoryDb`** – Required for certain mock retrieval operations.

---

## Notes

- These extensions simplify the process of generating mock request/response examples for endpoints.
- `Produces<T>(200)` ensures that the schema is globally registered, promoting reuse and reducing redundancy.
- This extension enhances testing and documentation by ensuring consistent, pre-generated examples across Swagger documentation.

