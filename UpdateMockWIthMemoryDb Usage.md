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
