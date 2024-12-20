using ApiDocAndMock.Infrastructure.Documentation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ApiDocAndMock.Infrastructure.Middleware
{
    /// <summary>
    /// Checks for mockOutcome querystring, and will mock an outcome of this depending on the value of the querystring provided when calling an Api endpoint
    /// Returns an Http response that has been queried from CommonResponseExamples by the value of the mockOutcome querystring
    /// </summary>
    public class MockOutcomeMiddleware
    {
        private readonly RequestDelegate _next;

        public MockOutcomeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.TryGetValue("mockOutcome", out var outcomeValue) &&
                int.TryParse(outcomeValue, out var statusCode))
            {
                // Map the status code to a predefined response
                var problemDetails = CommonResponseExamples.GetProblemDetailsForStatusCode(statusCode);
                if (problemDetails != null)
                {
                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                    return;
                }

                // If no predefined response, return a generic message
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Title = "Mock Outcome",
                    Status = statusCode,
                    Detail = $"Mock outcome triggered for status code {statusCode}."
                }));
                return;
            }

            // Proceed to the next middleware if no mockOutcome is specified
            await _next(context);
        }
    }
}
