﻿using Microsoft.AspNetCore.Http;
using System.Security.Claims;

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
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Mock User")
                };

                    // Retrieve Roles from Headers (X-Roles)
                    if (context.Request.Headers.TryGetValue("X-Roles", out var rolesHeader))
                    {
                        var roles = rolesHeader.ToString().Split(',');

                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                        }
                    }

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
