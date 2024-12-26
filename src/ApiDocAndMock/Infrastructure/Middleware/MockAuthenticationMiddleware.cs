using ApiDocAndMock.Infrastructure.Authorization;
using ApiDocAndMock.Shared.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ApiDocAndMock.Infrastructure.Middleware
{
    /// <summary>
    /// Simple Mock authentication implementation to enable Authorization for endpoints. Uses a hard coded mock-token for the bearer token. Only to be used for development and testing environments
    /// </summary>
    public class MockAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AuthMode _mode;
        public MockAuthenticationMiddleware(RequestDelegate next, IOptions<AuthSettings> settings)
        {
            _next = next;
            _mode = settings.Value.Mode;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.FirstOrDefault()?.Replace("Bearer ", string.Empty);

                switch (_mode)
                {
                    case AuthMode.BearerOnly:
                        if (string.Equals(token, "mock-token", StringComparison.OrdinalIgnoreCase))
                        {
                            context.User = CreateMockUser();
                        }
                        else
                        {
                            RejectRequest(context);
                            return;
                        }
                        break;

                    case AuthMode.XRolesHeader:
                        if (string.Equals(token, "mock-token", StringComparison.OrdinalIgnoreCase))
                        {
                            context.User = CreateMockUserWithRoles(context);
                        }
                        else
                        {
                            RejectRequest(context);
                            return;
                        }
                        break;

                    case AuthMode.JWTToken:
                        if (context.User.Identity?.IsAuthenticated != true)
                        {
                            RejectRequest(context);
                            return;
                        }
                        break;
                }
            }
            else if (_mode != AuthMode.JWTToken)
            {
                RejectRequest(context);
                return;
            }

            await _next(context);
        }

        private ClaimsPrincipal CreateMockUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Mock User")
            };
            var identity = new ClaimsIdentity(claims, "Mock");
            return new ClaimsPrincipal(identity);
        }

        private ClaimsPrincipal CreateMockUserWithRoles(HttpContext context)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Mock User")
            };

            if (context.Request.Headers.TryGetValue("X-Roles", out var rolesHeader))
            {
                var roles = rolesHeader.ToString().Split(',');

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                }
            }

            var identity = new ClaimsIdentity(claims, "Mock");
            return new ClaimsPrincipal(identity);
        }

        private void RejectRequest(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.Add("WWW-Authenticate", @"Bearer error=""invalid_token""");
        }
    }
}
