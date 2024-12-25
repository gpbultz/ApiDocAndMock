
using ApiDocAndMock.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using TestApi.Application.Queries.Contacts;
using TestApi.Application.Queries.Hotels;
using TestApi.Domain.Entities;
using TestApi.Infrastructure.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDocAndMock();
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

builder.Services.AddDefaultFakerRules(rules =>
{
    rules["Phone"] = faker => "+44 " + faker.Phone.PhoneNumber();  // UK-specific format
});

builder.Services.AddMockingConfigurations(config =>
{
    // Booking configuration
    config.RegisterConfiguration<Booking>(cfg =>
    {
        cfg
            .ForPropertyObject<Room>("Room")
            .ForPropertyObject<Contact>("PrimaryContact");
    });

    // Hotel configuration
    config.RegisterConfiguration<Hotel>(cfg =>
    {
        cfg
            .ForProperty("Name", faker => faker.Company.CompanyName())
            .ForPropertyObjectList<Room>("Rooms", 5)
            .ForPropertyObjectList<Booking>("Bookings", 5);
    });

    config.RegisterConfiguration<GetContactsResponse>(cfg =>
    {
        cfg
            .ForPropertyObjectList<GetContactByIdResponse>("Contacts", 5);
    });

    config.RegisterConfiguration<GetHotelsResponse>(cfg =>
    {
        cfg
            .ForPropertyObjectList<GetHotelByIdResponse>("Hotels", 5);
    });

    config.RegisterConfiguration<GetHotelByIdResponse>(cfg =>
    {
        cfg
            .ForProperty("Name", faker => faker.Company.CompanyName())
            .ForPropertyObjectList<Room>("Rooms", 5)
            .ForPropertyObjectList<Booking>("Bookings", 5);
    });

});

var app = builder.Build();


app.UseApiDocAndMock(useAuthentication: true, useMockOutcome: true);

app.UseHttpsRedirection();
app.MapContactEndpoints();
app.MapHotelEndpoints();
app.MapBookingEndpoints();

app.UseSwagger();
app.UseSwaggerUI();


app.Run();
