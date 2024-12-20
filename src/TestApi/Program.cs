
using ApiDocAndMock.Infrastructure.Extensions;
using ApiDocAndMock.Infrastructure.Mocking;
using Microsoft.AspNetCore.Mvc;
using NSwagDemo.Infrastructure.API.Extensions;
using TestApi.Application.Queries.Contacts;

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
    config.RegisterConfiguration<GetContactsResponse>(responseConfig =>
    {
        responseConfig
            .ForProperty("TotalCount", faker => faker.Random.Int(50, 100))
            .ForProperty("PageNumber", faker => faker.Random.Int(1, 10))
            .ForProperty("PageSize", faker => faker.Random.Int(10, 50))
            .ForNestedProperty<GetContactByIdResponse>("Contacts", nestedCount: 5);
    });
});

var app = builder.Build();
app.UseApiMockAndDock();

//app.UseHttpsRedirection();
app.MapContactEndpoints();
//app.MapHotelEndpoints();
//app.MapBookingEndpoints();

app.UseSwagger();
app.UseSwaggerUI();


app.Run();
