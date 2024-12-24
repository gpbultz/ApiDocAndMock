using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Extensions;
using ApiDocAndMock.Infrastructure.Mocking;
using ApiDocAndMock.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using TestApi.Application.Commands.Contacts;
using TestApi.Application.Queries.Contacts;

namespace TestApi.Infrastructure.API.Extensions
{
    public static class ContactApiExtensions
    {
        public static void MapContactEndpoints(this IEndpointRouteBuilder app)
        {
            var serviceProvider = app.ServiceProvider;
            var mockDataFactory = serviceProvider.GetRequiredService<IApiMockDataFactory>();

            // Get All Contacts
            app.MapGet("/contacts", ([FromServices] IMemoryDb db, [AsParameters] GetContactsQuery query) =>
            {
                var response = mockDataFactory
                                    .CreateMockObject<GetContactsResponse>(query.PageSize ?? 10)
                                    .WithPaginationAndLinks(
                                        totalCount: 100,
                                        pageSize: query.PageSize ?? 10,
                                        currentPage: query.Page ?? 1,
                                        resourcePath: "/contacts"
                                    ); 

                return Results.Ok(response);
            })
            .Produces<GetContactsResponse>(200)
            .WithEnrichedMockedResponse<GetContactsResponse>(includePages: true)       // Document response
            .WithSummary("Retrieve all contacts", "Returns a paginated list of contacts with optional filtering by city.")
            .RequireBearerToken()
            .WithCommonResponses("401", "403", "429", "500")
            .WithRequiredQueryParameter("City", "City is required for filtering contacts.")
            .WithMockOutcome();


            // Get Contact by ID
            app.MapGet("/contacts/{id:guid}", (Guid id) => Results.Ok())
            .GetMockFromMemoryDb<GetContactByIdResponse>()
            .Produces<GetContactByIdResponse>(200)
            .Produces(404)
            .WithMockResponse<GetContactByIdResponse>()
            .RequireBearerToken()
            .WithSummary("Retrieve a contact by ID", "Returns a single contact by ID, or mock data if not found.")
            .WithCommonResponses("401", "429", "500");

            // Update Contact by Id
            app.MapPut("/contacts/{id:guid}", ([FromBody] UpdateContactCommand command) =>
            {
                return Results.Ok(new UpdateContactResponse { Result = "updated" });
            })
            .WithMockRequest<UpdateContactCommand>()
            .WithStaticResponse("201", new UpdateContactResponse { Result = "updated" })
            .WithPathParameter(name: "id", description: "The unique identifier of the contact to update.")
            .UpdateMockWithMemoryDb<UpdateContactCommand, GetContactByIdResponse, UpdateContactResponse>(responseMapper: obj => new UpdateContactResponse { Result = "success"})
            .RequireBearerToken()
            .Produces<UpdateContactResponse>(200)
            .Produces<UpdateContactResponse>(201)
            .Produces(204)
            .WithCommonResponses("400", "401", "429", "500")
            .WithSummary("Update existing contact", "Updates an existing contact or creates new contact if queried contact does not exist");

            //Delete contact by Id
            app.MapDelete("/contacts/{id:guid}", (Guid id) =>
            {
                return Results.NoContent();
            })
            .WithPathParameter(name: "Id", description: "The unique identifier of the contact to delete.")
            .DeleteMockWithMemoryDb<GetContactByIdResponse, DeleteContactResponse>()
            .RequireBearerToken();
            


            // Create Contact
            app.MapPost("/contacts", ([FromBody] CreateContactCommand command) =>
            {
                return Results.Ok(command);
            })
            .CreateMockWithMemoryDb<CreateContactCommand, GetContactByIdResponse, CreateContactResponse>()
            .Produces<CreateContactResponse>(201)
            .WithMockRequest<CreateContactCommand>()
            .WithMockResponse<CreateContactResponse>()
            .WithValidationErrors<CreateContactCommand>()
            .RequireBearerToken()
            .WithSummary("Create a new contact", "Creates a new contact with the provided details.")
            .WithCommonResponses("401", "429", "500");
        }
    }


}
