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
            // Get All Contacts
            app.MapGet("/contacts", ([FromServices] IMemoryDb db, [AsParameters] GetContactsQuery query) =>
            {
                var response = ApiMockDataFactory.CreateMockObject<GetContactsResponse>(query.PageSize ?? 10);
                response.PageNumber = query.Page ?? 1;
                response.PageSize = query.PageSize ?? 10;
                response.TotalCount = 50;

                return Results.Ok(response);
            })
            .Produces<GetContactsResponse>(200)
            .WithMockResponse<GetContactsResponse>()       // Document response
            .WithSummary("Retrieve all contacts", "Returns a paginated list of contacts with optional filtering by city.")
            .RequireBearerToken()
            .WithCommonResponses("401", "403", "429", "500")
            .WithRequiredQueryParameter("City", "City is required for filtering contacts.")
            .WithMockOutcome();


            // Get Contact by ID
            app.MapGet("/contacts/{id:guid}", (Guid id) => Results.Ok())
            .GetMockFromMemoryDb<GetContactByIdResponse>(idFieldName: "Id", defaultBehaviour: NotFoundBehaviour.Return404)
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
            .UpdateMockWithMemoryDb<UpdateContactCommand, GetContactByIdResponse, UpdateContactResponse>()
            .RequireBearerToken()
            .Produces<UpdateContactResponse>(200)
            .Produces<UpdateContactResponse>(201)
            .Produces(204)
            .WithCommonResponses("400", "401", "429", "500")
            .WithSummary("Update existing contact", "Updates an existing contact or creates new contact if queried contact does not exist");

            //Delete contact by Id
            app.MapDelete("/contacts/{id:guid}", () =>
            {
                return Results.NoContent();
            })
            .WithPathParameter(name: "id", description: "The unique identifier of the contact to delete.")
            .DeleteMockWithMemoryDb<GetContactByIdResponse, DeleteContactResponse>(
                responseMapper: deleted => new DeleteContactResponse
                {
                    Status = "Deleted successfully.",
                    DeletedId = deleted.Id
                });


            // Create Contact
            app.MapPost("/contacts", ([FromBody] CreateContactCommand command) =>
            {
                return Results.Ok(command);
            })
            .CreateMockWithMemoryDb<CreateContactCommand, GetContactByIdResponse, CreateContactResponse>()
            //.CreateMockWithMemoryDb<CreateContactCommand, GetContactByIdResponse, CreateContactResponse>(
            //    customMapper: command => new GetContactByIdResponse
            //    {
            //        Id = Guid.NewGuid(),
            //        Name = $"{command.Name} (Custom)",
            //        Email = command.Email,
            //        City = command.City
            //    },
            //    locationPathBuilder: response => $"/contacts/{response.Id}")
            .Produces<CreateContactResponse>(201)
            .WithMockRequest<CreateContactCommand>()
            .WithMockResponse<CreateContactResponse>()
            .WithValidationErrors<CreateContactCommand>()
            .WithSummary("Create a new contact", "Creates a new contact with the provided details.")
            .WithCommonResponses("401", "429", "500");
        }
    }


}
