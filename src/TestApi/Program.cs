
using ApiDocAndMock.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using TestApi.Application.Queries.Contacts;
using TestApi.Application.Queries.Hotels;
using TestApi.Domain.Entities;
using TestApi.Infrastructure.API;
using TestApi.Infrastructure.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDocAndMock();

builder.Services.AddMockAuthentication(authMode: ApiDocAndMock.Shared.Enums.AuthMode.BearerOnly);

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

//builder.Services.SetDefaultFakerRules(defaultRules =>
//{
//    defaultRules["Name"] = faker => faker.Name.FullName();
//    defaultRules["Email"] = faker => faker.Internet.Email();
//    defaultRules["Address"] = faker => faker.Address.FullAddress();
//    defaultRules["Phone"] = faker => faker.Phone.PhoneNumber();
//});

builder.Services.AddMockingConfigurations(config =>
{
    config.RegisterConfiguration<Appointment>(cfg =>
    {
        cfg
            .ForProperty("DateOfAppointment", faker => faker.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now))
            .ForProperty("Description", faker => "Meeting with regards to " + faker.Commerce.Department());
    });

    // Booking configuration
    config.RegisterConfiguration<Booking>(cfg =>
    {
        cfg
            .ForPropertyObject<Room>("Room")
            .ForPropertyObject<Contact>("PrimaryContact");
    });

    config.RegisterConfiguration<GetContactByIdResponse>(cfg =>
    {
        cfg.ForPropertyDictionary<Guid, Appointment>("Appointments", 3, faker => Guid.NewGuid());
    });

    // Hotel configuration
    config.RegisterConfiguration<Hotel>(cfg =>
    {
        cfg
            .ForProperty("Name", faker => faker.Company.CompanyName())
            .ForPropertyTuple("Coordinates", faker => faker.Address.Latitude(), faker=> faker.Address.Longitude())
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
            .ForPropertyTuple("Coordinates", faker => faker.Address.Latitude(), faker => faker.Address.Longitude())
            .ForPropertyObjectList<Room>("Rooms", 5)
            .ForPropertyObjectList<Booking>("Bookings", 5)
            .ForPropertyDictionary("Metadata", 3, faker => Guid.NewGuid(), faker => faker.Commerce.Product(), isPrimitive: true);
    });

});

var app = builder.Build();


app.UseApiDocAndMock(useAuthentication: true, useMockOutcome: true);

app.UseHttpsRedirection();
app.MapContactEndpoints();
app.MapHotelEndpoints();
app.MapBookingEndpoints();
app.MapJwtTokenEndpoints();

app.UseSwagger();
app.UseSwaggerUI();


app.Run();
