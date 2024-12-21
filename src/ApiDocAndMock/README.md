
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

[[OpenApiExtensions For MinimalApi endpoints.md]]
[[OpenApiMockExtensions For MinimalApi Endpoints.md]]
[[GetMockFromMemoryDb Usage.md]]
[[UpdateMockWIthMemoryDb Usage.md]]
[[CreateMockWithMemoryDb Usage.md]]
[[DeleteMockWithMemoryDb Usage.md]]
