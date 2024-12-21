using ApiDocAndMock.Infrastructure.Extensions;
using ApiDocAndMock.Infrastructure.Mocking;
using TestApi.Application.Queries.Hotels;
using TestApi.Domain.Entities;

namespace TestApi.Infrastructure.API.Extensions
{
    public static class HotelApiExtensions
    {
        public static void MapHotelEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/hotels", ([AsParameters] GetHotelsQuery query) =>
            {
                var hotels = ApiMockDataFactory.CreateMockObject<GetHotelsResponse>();
                hotels.PageNumber = query.Page ?? 1;
                hotels.PageSize = query.PageSize ?? 10;
                hotels.TotalCount = 50;
                return Results.Ok(hotels);
            })
            .Produces<GetHotelsResponse>(200)
            .WithMockResponseList<GetHotelsResponse>(count: 5)
            .RequireBearerToken()
            .WithSummary("Retrieve hotels", "Returns a paginated list of hotels.")
            .WithCommonResponses("401", "429", "500");

            //app.MapGet("/hotels/{id}", (Guid id) =>
            //{
            //    var hotel = ApiMockDataFactory.CreateMockObjects<Hotel>(count: 1);
            //    hotel.FirstOrDefault().Id = id; // Assign ID from path
            //    return Results.Ok(hotel);
            //})
            //.Produces<Hotel>(200)
            //.WithMockResponse<Hotel>()
            //.RequireBearerToken()
            //.WithSummary("Retrieve a specific hotel", "Returns the details of a hotel by its ID.")
            //.WithCommonResponses("401", "429", "500");
        }
    }

}
