﻿using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using TestApi.Domain.Entities;


namespace NSwagDemo.Infrastructure.API.Extensions
{
    public static class BookingApiExtensions
    {
        public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings", ([FromServices] IApiMockDataFactory mockDataFactory) =>
            {
                var bookings = mockDataFactory.CreateMockObjects<Booking>(count: 20);
                return Results.Ok(bookings);
            })
            .WithMockResponseList<Booking>(count: 20)
            .RequireBearerToken()
            .WithSummary("Retrieve bookings", "Returns a list of bookings with mock data for testing.");

            app.MapPost("/bookings", (Booking booking) =>
            {
                booking.Id = Guid.NewGuid(); // Simulate ID assignment
                return Results.Created($"/bookings/{booking.Id}", booking);
            })
            .WithMockRequest<Booking>()
            .RequireBearerToken()
            .WithSummary("Create a new booking", "Creates a new booking with the provided details.");
        }
    }

}