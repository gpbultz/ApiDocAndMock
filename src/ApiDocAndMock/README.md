
# ApiMockDataFactory

A comprehensive API mocking utility for .NET projects, leveraging the power of **Bogus** for seamless data generation.

---

## Installation

Install the package via NuGet:

```bash
dotnet add package ApiDocAndMock
```

---

## Usage

### Service Configuration
Integrate the package with your application by adding the required services:

```csharp
// Main service setup
builder.Services.AddDocAndMock();

// Optional: Add authentication for API mocking
builder.Services.AddMockAuthentication();

// Swagger integration for documentation and mock APIs
builder.Services.AddMockSwagger(includeSecurity: true, includAnnotations: true);

// Optional: Add an in-memory database for simulating CRUD operations
builder.Services.AddMemoryDb();
```

### Application Middleware
Configure the application middleware to enable API documentation and mocking:

```csharp
var app = builder.Build();

// Enable API documentation and mock utilities
app.UseApiDocAndMock(useAuthentication: true, useMockOutcome: true);

// Enable Swagger for API visualization
app.UseSwagger();
app.UseSwaggerUI();
```

---

## Configuration

### Response Types
Customize default response types or add new ones for specific HTTP status codes:

```csharp
builder.Services.AddCommonResponseConfigurations(config =>
{
    config.RegisterResponseExample(500, new ProblemDetails
    {
        Title = "Custom Internal Server Error",
        Status = 500,
        Detail = "A custom error occurred. Please contact support."
    });

    config.RegisterResponseExample(403, new ProblemDetails
    {
        Title = "Forbidden",
        Status = 403,
        Detail = "You do not have permission to access this resource."
    });
});
```

### Default Faker Rules
Define default faker rules to override existing rules and generate mock data for specific property names:

```csharp
builder.Services.AddDefaultFakerRules(rules =>
{
    rules["Phone"] = faker => "+44 " + faker.Phone.PhoneNumber(); 
});
```

### Mocking Configurations
Set up custom mocking configurations to apply specific rules to request and response objects. This ensures that objects adhere to predefined formats, with fallback defaults provided by **Bogus**.

```csharp
builder.Services.AddMockingConfigurations(config =>
{
    // Configure Booking object
    config.RegisterConfiguration<Booking>(cfg =>
    {
        cfg
            .ForPropertyObject<Room>("Room")
            .ForPropertyObject<Contact>("PrimaryContact");
    });

    // Configure Hotel object
    config.RegisterConfiguration<Hotel>(cfg =>
    {
        cfg
            .ForProperty("Name", faker => faker.Company.CompanyName())
            .ForPropertyObjectList<Room>("Rooms", 5)
            .ForPropertyObjectList<Booking>("Bookings", 5);
    });
});
```

### Notes on Mocking
- **ForPropertyObject**: Applies rules to a specific nested object within a response.
- **ForPropertyObjectList**: Configures a list of nested objects with a specified number of items.
- If a property lacks an explicit rule, a default value is assigned based on its type. Undefined types default to `null`.

# OpenApiExtensions For MinimalApi endpoints
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

---

### 10. `WithAuthorizationRoles`

**Description**: Adds role-based authorization to endpoints and documents the required roles in Swagger.

**Parameters**:

- `roles` _(params string[])_ – List of required roles for the endpoint.

**Usage**:

```csharp
app.MapGet("/api/secure", () => Results.Ok())
   .WithAuthorizationRoles("Admin", "Manager");
```

**OpenAPI Effect**:

- Requires specified roles for the endpoint.
- If the authorization mode is `XRolesHeader`, adds `X-Roles` to the header documentation in Swagger.

**Notes**:
- Supports both JWT-based role checking and custom `X-Roles` header authorization.
- Updates Swagger to document the required roles and ensure proper authorization behavior.

---

# OpenApiMockExtensions For MinimalApi Endpoints
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

### 2. Response Integration

- The generated mock objects are inserted directly into the `OpenApiResponse` or `OpenApiRequestBody`.
- The schema for the mock objects is referenced in the response to ensure consistency in Swagger.

---

## Dependencies

- **`ApiMockDataFactory`** – A static factory responsible for creating mock objects of the specified type.
- **`IMemoryDb`** – Required for certain mock retrieval operations.

---

## Notes

- These extensions simplify the process of generating mock request/response examples for endpoints.
- They enhance testing and documentation by ensuring consistent, pre-generated examples across Swagger documentation.
- `Produces<T>(200)` ensures that the schema is globally registered, promoting reuse and reducing redundancy.

# GetMockFromMemoryDb Usage
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

# UpdateMockWithMemoryDb Usage
The UpdateMockWithMemoryDb method is an extension for RouteHandlerBuilder that facilitates mock data updates in an in-memory database (IMemoryDb) through API endpoints. It enables handling dynamic mapping of request data to stored entities and provides flexibility in customizing response objects.

Method Signature

```csharp
public static RouteHandlerBuilder UpdateMockWithMemoryDb<TRequest, TStored, TResponse>(
    this RouteHandlerBuilder builder,
    string sourceIdFieldName = "Id",
    string queryIdFieldName = "Id",
    Func<TRequest, TStored>? customMapper = null,
    Func<TStored, TResponse>? responseMapper = null,
    string? defaultMethodOutcome = "Return200",
    string? locationPath = "")
    where TRequest : class
    where TStored : class, new()
    where TResponse : class, new()
```

### Key Parameters

- **`customMapper`** (`Func<TRequest, TStored>?`):
    
    - A function that maps the incoming request (`TRequest`) to the stored object (`TStored`). This overrides the default mapping logic.
- **`responseMapper`** (`Func<TStored, TResponse>?`):
    
    - A function that transforms the stored object (`TStored`) into a response object (`TResponse`). This allows customization of the API response.
- **`sourceIdFieldName`**: The name of the field in the incoming request that identifies the object.
    
- **`queryIdFieldName`**: The name of the field used to query the in-memory database.
    
- **`defaultMethodOutcome`**: Controls the HTTP response code (e.g., `200`, `201`, `204`).
    
- **`locationPath`**: Specifies the path for `201 Created` responses.
    

## Example Use Cases

### 1. Basic Update without Custom Mapping

```csharp
app.MapPost("/updateContact", (ContactRequest request) =>
{
    return Results.Ok();
})
.UpdateMockWithMemoryDb<ContactRequest, Contact, ContactResponse>();
```

- **Behavior:**
    - Maps the properties from `ContactRequest` to `Contact` using default mapping logic.
    - Returns a `ContactResponse` created by `ApiMockDataFactory`.

### 2. Custom Mapping of Request to Stored Object

```csharp
app.MapPost("/updateBooking", (BookingRequest request) =>
{
    return Results.Ok();
})
.UpdateMockWithMemoryDb<BookingRequest, Booking, BookingResponse>(
    customMapper: (request) => new Booking
    {
        Id = request.BookingId,
        DateFrom = request.StartDate,
        DateTo = request.EndDate
    }
);
```

- **Behavior:**
    - Maps `BookingRequest` fields manually to `Booking`.
    - Allows full control over the stored object creation.

### 3. Custom Response Mapping

```csharp
app.MapPost("/createRoom", (RoomRequest request) =>
{
    return Results.Ok();
})
.UpdateMockWithMemoryDb<RoomRequest, Room, RoomResponse>(
    responseMapper: (stored) => new RoomResponse
    {
        RoomId = stored.Id,
        RoomNumber = stored.RoomNumber,
        Floor = stored.Floor
    }
);
```

- **Behavior:**
    - Customizes the API response to return specific fields from the stored object.
    - The `RoomResponse` object is tailored to the application's needs.

### 4. Full Customization

```csharp
app.MapPost("/updateHotel", (HotelRequest request) =>
{
    return Results.Ok();
})
.UpdateMockWithMemoryDb<HotelRequest, Hotel, HotelResponse>(
    customMapper: (request) => new Hotel { Name = request.HotelName },
    responseMapper: (stored) => new HotelResponse { HotelName = stored.Name }
);
```

- **Behavior:**
    - Customizes both the mapping to the stored entity (`Hotel`) and the response mapping (`HotelResponse`).

## How It Works

1. The incoming request is processed by extracting the `Id` field (or a custom identifier).
2. The in-memory database (`IMemoryDb`) is queried for an existing object with the matching identifier.
3. If the object is found, it is updated. If not, a new instance of the stored object is created.
4. If `customMapper` is defined, it applies the custom mapping logic. Otherwise, the default property mapping (`MapRequestToStored`) is used.
5. The stored object is updated in the in-memory database.
6. A response object (`TResponse`) is generated through the `responseMapper` or by using a mock object factory (`ApiMockDataFactory`).
7. The HTTP method outcome (`200`, `201`, `204`) is determined by the `methodOutcome` query parameter or the default.

## Notes

- **`customMapper`** is useful for complex object transformations that cannot be handled by default property mapping.
- **`responseMapper`** is helpful when the response object requires calculated fields or aggregated data.
- If neither mapper is specified, the default behavior applies basic property mapping and creates a response using `ApiMockDataFactory`.

# CreateMockWithMemoryDb Usage
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


# DeleteMockWithMemoryDb Usage
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

