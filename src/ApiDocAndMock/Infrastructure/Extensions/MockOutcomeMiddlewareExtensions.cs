using ApiDocAndMock.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    /// <summary>
    /// Application builder to enable MockOutcomeMiddleware, which will check for queryString parameter mockOutcome and use the value as the HTTP status code to return without reaching the endpoint
    /// </summary>
    public static class MockOutcomeMiddlewareExtensions
    {
        public static IApplicationBuilder UseMockOutcome(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MockOutcomeMiddleware>();
        }
    }
}
