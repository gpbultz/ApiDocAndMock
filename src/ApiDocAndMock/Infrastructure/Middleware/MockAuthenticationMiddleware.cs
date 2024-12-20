using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Middleware
{
    /// <summary>
    /// Simple Mock authentication implementation to enable Authorization for endpoints. Uses a hard coded mock-token for the bearer token. Only to be used for development and testing environments
    /// </summary>
    public class MockAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public MockAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.FirstOrDefault()?.Replace("Bearer ", string.Empty);

                if (string.Equals(token, "mock-token", StringComparison.OrdinalIgnoreCase))
                {
                    var claims = new[]
                    {
                    new Claim(ClaimTypes.Name, "Mock User"),
                    new Claim(ClaimTypes.Role, "User"),
                };

                    var identity = new ClaimsIdentity(claims, "Mock");
                    context.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.Add("WWW-Authenticate", @"Bearer error=""invalid_token""");
                    return;
                }
            }

            await _next(context);
        }
    }
}
