using ApiDocAndMock.Application.Models.Requests;
using ApiDocAndMock.Application.Models.Responses;
using ApiDocAndMock.Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace TestApi.Infrastructure.API
{
    public static class JwtTokenApiExtensions
    {
        public static void MapJwtTokenEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/token", ([FromBody] JwtTokenRequest command) =>
            {
                var response = new JwtTokenResponse();
                response.Token = JwtTokenGenerator.GenerateMockJwt(command.Roles.ToArray());

                return Results.Ok(response);
            });
        }
    }
}
