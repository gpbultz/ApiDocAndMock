
using ApiDocAndMock.Infrastructure.Extensions;
using ApiDocAndMock.Infrastructure.Mocking;
using Microsoft.AspNetCore.Mvc;
using TestApi.Infrastructure.API.Extensions;
using TestApi.Application.Queries.Contacts;
using TestApi.Application.Queries.Hotels;
using TestApi.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMockAuthentication();
builder.Services.AddMockSwagger(includeSecurity: true, includAnnotations: true);
builder.Services.AddMemoryDb();

builder.Services.AddCommonResponseConfigurations(config =>
{
    config.RegisterResponseExample(500, new ProblemDetails
    {
        Title = "Custom Internal Server Error",
        Status = 500,
        Detail = "A custom error occurred. Please contact support."
    });

    // Add a custom response example for 403
    config.RegisterResponseExample(403, new ProblemDetails
    {
        Title = "Forbidden",
        Status = 403,
        Detail = "You do not have permission to access this resource."
    });
});

builder.Services.AddMockingConfigurations(config =>
{
    config.RegisterConfiguration<Contact>(cfg =>
    {
        cfg
            .ForProperty("Id", faker => Guid.NewGuid())
            .ForProperty("Name", faker => faker.Name.FullName())
            .ForProperty("Email", faker => faker.Internet.Email())
            .ForProperty("Phone", faker => faker.Phone.PhoneNumber())
            .ForProperty("Address", faker => faker.Address.FullAddress())
            .ForProperty("City", faker => faker.Address.City())
            .ForProperty("Region", faker => faker.Address.StateAbbr())
            .ForProperty("PostalCode", faker => faker.Address.ZipCode());
    });

    // Booking configuration
    config.RegisterConfiguration<Booking>(cfg =>
    {
        cfg
            .ForProperty("Id", faker => Guid.NewGuid())
            .ForProperty("DateFrom", faker => faker.Date.Soon())
            .ForProperty("DateTo", faker => faker.Date.Soon(10))
            .ForProperty("NumberOfGuests", faker => faker.Random.Int(1, 5))
            .ForPropertyObject<Room>("Room")
            .ForPropertyObject<Contact>("PrimaryContact");
    });

    // Hotel configuration
    config.RegisterConfiguration<Hotel>(cfg =>
    {
        cfg
            .ForProperty("Id", faker => Guid.NewGuid())
            .ForProperty("Name", faker => faker.Company.CompanyName())
            .ForProperty("Address", faker => faker.Address.FullAddress())
            .ForProperty("City", faker => faker.Address.City())
            .ForProperty("Region", faker => faker.Address.StateAbbr())
            .ForProperty("PostalCode", faker => faker.Address.ZipCode())
            .ForProperty("Country", faker => faker.Address.Country())
            .ForProperty("Phone", faker => faker.Phone.PhoneNumber())
            .ForPropertyObjectList<Room>("Rooms", 5)
            .ForPropertyObjectList<Booking>("Bookings", 5);
    });

    config.RegisterConfiguration<Room>(cfg =>
    {
        cfg.ForProperty("Floor", faker => faker.Random.Int(1, 10))
                  .ForProperty("RoomNumber", faker => faker.Random.Int(100, 999))
                  .ForProperty("NumberOfBeds", faker => faker.Random.Int(1, 3));
    });

    config.RegisterConfiguration<GetContactsQuery>(cfg =>
    {
        cfg.ForProperty("Page", faker => faker.Random.Int(1, 100))
           .ForProperty("PageSize", faker => faker.Random.Int(10, 50))
           .ForProperty("City", faker => faker.Address.City());
    });

    config.RegisterConfiguration<GetContactByIdResponse>(cfg =>
    {
        cfg.ForProperty("Name", faker => faker.Name.FullName())
            .ForProperty("Email", faker => faker.Internet.Email())
            .ForProperty("Phone", faker => faker.Phone.PhoneNumber())
            .ForProperty("Address", faker => faker.Address.FullAddress())
            .ForProperty("City", faker => faker.Address.City())
            .ForProperty("Region", faker => faker.Address.StateAbbr())
            .ForProperty("PostalCode", faker => faker.Address.ZipCode());
    });

    config.RegisterConfiguration<GetContactsResponse>(cfg =>
    {
        cfg
            .ForProperty("TotalCount", faker => faker.Random.Int(50, 100))
            .ForProperty("PageNumber", faker => faker.Random.Int(1, 10))
            .ForProperty("PageSize", faker => faker.Random.Int(10, 50))
            .ForPropertyObjectList<GetContactByIdResponse>("Contacts", 5);
    });

    config.RegisterConfiguration<GetHotelsResponse>(cfg =>
    {
        cfg
            .ForProperty("TotalCount", faker => faker.Random.Int(1, 10))
            .ForProperty("PageNumber", faker => faker.Random.Int(1, 1))
            .ForProperty("PageNumber", faker => faker.Random.Int(1, 1))
            .ForProperty("PageSize", faker => faker.Random.Int(1, 1))
            .ForPropertyObjectList<GetHotelByIdResponse>("Hotels", 5);
    });

    config.RegisterConfiguration<GetHotelByIdResponse>(cfg =>
    {
        cfg
            .ForProperty("Id", faker => faker.Random.Guid())
            .ForProperty("Rooms", faker => ApiMockDataFactory.CreateMockObjects<Room>(5))
            .ForProperty("Name", faker => faker.Company.CompanyName())
            .ForProperty("Address", faker => faker.Address.FullAddress())
            .ForProperty("City", faker => faker.Address.City())
            .ForProperty("Region", faker => faker.Address.StateAbbr())
            .ForProperty("PostalCode", faker => faker.Address.ZipCode())
            .ForProperty("Country", faker => faker.Address.Country())
            .ForProperty("Phone", faker => faker.Phone.PhoneNumber())
            .ForPropertyObjectList<Booking>("Bookings", 5);
    });

});

var app = builder.Build();


app.UseApiDocAndMock(useAuthentication: true, useMockOutcome: true);

//app.UseHttpsRedirection();
app.MapContactEndpoints();
app.MapHotelEndpoints();
app.MapBookingEndpoints();

app.UseSwagger();
app.UseSwaggerUI();


app.Run();
