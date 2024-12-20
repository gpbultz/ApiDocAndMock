
using ApiDocAndMock.Infrastructure.Extensions;
using ApiDocAndMock.Infrastructure.Mocking;
using Microsoft.AspNetCore.Mvc;
using NSwagDemo.Infrastructure.API.Extensions;
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

builder.Services.AddMockingConfigurations(() =>
{
    MockConfigurationsFactory.RegisterConfiguration<Contact>(contactConfig =>
    {
        contactConfig
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
    MockConfigurationsFactory.RegisterConfiguration<Booking>(bookingConfig =>
    {
        bookingConfig
            .ForProperty("Id", faker => Guid.NewGuid())
            .ForProperty("DateFrom", faker => faker.Date.Soon())
            .ForProperty("DateTo", faker => faker.Date.Soon(10))
            .ForProperty("NumberOfGuests", faker => faker.Random.Int(1, 5))
            .ForProperty("Room", faker => ApiMockDataFactory.CreateMockObject<Room>())
            .ForProperty("PrimaryContact", faker => ApiMockDataFactory.CreateMockObject<Contact>());
    });

    // Hotel configuration
    MockConfigurationsFactory.RegisterConfiguration<Hotel>(hotelConfig =>
    {
        hotelConfig
            .ForProperty("Id", faker => Guid.NewGuid())
            .ForProperty("Name", faker => faker.Company.CompanyName())
            .ForProperty("Address", faker => faker.Address.FullAddress())
            .ForProperty("City", faker => faker.Address.City())
            .ForProperty("Region", faker => faker.Address.StateAbbr())
            .ForProperty("PostalCode", faker => faker.Address.ZipCode())
            .ForProperty("Country", faker => faker.Address.Country())
            .ForProperty("Phone", faker => faker.Phone.PhoneNumber())
            .ForProperty("Rooms", faker => ApiMockDataFactory.CreateMockObjects<Room>(5))
            .ForProperty("Bookings", faker => ApiMockDataFactory.CreateMockObjects<Booking>(5));
    });
    MockConfigurationsFactory.RegisterConfiguration<Room>(roomConfig =>
    {
        roomConfig.ForProperty("Floor", faker => faker.Random.Int(1, 10))
                  .ForProperty("RoomNumber", faker => faker.Random.Int(100, 999))
                  .ForProperty("NumberOfBeds", faker => faker.Random.Int(1, 3));
    });
    MockConfigurationsFactory.RegisterConfiguration<GetContactsQuery>(cfg =>
    {
        cfg.ForProperty("Page", faker => faker.Random.Int(1, 100))
           .ForProperty("PageSize", faker => faker.Random.Int(10, 50))
           .ForProperty("City", faker => faker.Address.City());
    });
    MockConfigurationsFactory.RegisterConfiguration<GetContactByIdResponse>(cfg =>
    {
        cfg.ForProperty("Name", faker => faker.Name.FullName())
            .ForProperty("Email", faker => faker.Internet.Email())
            .ForProperty("Phone", faker => faker.Phone.PhoneNumber())
            .ForProperty("Address", faker => faker.Address.FullAddress())
            .ForProperty("City", faker => faker.Address.City())
            .ForProperty("Region", faker => faker.Address.StateAbbr())
            .ForProperty("PostalCode", faker => faker.Address.ZipCode());
    });
    MockConfigurationsFactory.RegisterConfiguration<GetContactsResponse>(responseConfig =>
    {
        responseConfig
            .ForProperty("TotalCount", faker => faker.Random.Int(50, 100))
            .ForProperty("PageNumber", faker => faker.Random.Int(1, 10))
            .ForProperty("PageSize", faker => faker.Random.Int(10, 50))
            .ForProperty("Contacts", faker => ApiMockDataFactory.CreateMockObjects<GetContactByIdResponse>(5));
    });
    MockConfigurationsFactory.RegisterConfiguration<GetHotelsResponse>(responseConfig =>
    {
        responseConfig
            .ForProperty("TotalCount", faker => faker.Random.Int(1, 10))
            .ForProperty("PageNumber", faker => faker.Random.Int(1, 1))
            .ForProperty("PageNumber", faker => faker.Random.Int(1, 1))
            .ForProperty("PageSize", faker => faker.Random.Int(1, 1))
            .ForProperty("Hotels", faker => ApiMockDataFactory.CreateMockObjects<GetHotelByIdResponse>(5));
    });

    MockConfigurationsFactory.RegisterConfiguration<GetHotelByIdResponse>(cfg =>
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
            .ForProperty("Bookings", faker => ApiMockDataFactory.CreateMockObjects<Booking>(5));
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
