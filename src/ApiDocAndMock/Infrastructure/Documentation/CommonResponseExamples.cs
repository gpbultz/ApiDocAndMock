using Microsoft.AspNetCore.Mvc;

namespace ApiDocAndMock.Infrastructure.Documentation
{
    /// <summary>
    /// A collection of Common responses that will be used for Swagger Documentation
    /// </summary>
    public static class CommonResponseExamples
    {
        public static ProblemDetails BadRequestExample => new ProblemDetails
        {
            Title = "Bad Request",
            Status = 400,
            Detail = "Bad Request - Malformed request syntax or invalid JSON."
        };

        public static ProblemDetails UnauthorizedExample => new ProblemDetails
        {
            Title = "Unauthorized",
            Status = 401,
            Detail = "Authentication is required to access this resource."
        };

        public static ProblemDetails RateLimitExceededExample => new ProblemDetails
        {
            Title = "Too Many Requests",
            Status = 429,
            Detail = "You have exceeded the allowed number of requests. Please try again later."
        };

        public static ProblemDetails InternalServerErrorExample => new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = 500,
            Detail = "An unexpected error occurred. Please try again later."
        };

        public static ProblemDetails? GetProblemDetailsForStatusCode(int statusCode) => statusCode switch
        {
            400 => BadRequestExample,
            401 => UnauthorizedExample,
            429 => RateLimitExceededExample,
            500 => InternalServerErrorExample,
            _ => null // Return null for undefined status codes
        };
    }
}
