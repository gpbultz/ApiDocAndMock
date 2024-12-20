using ApiDocAndMock.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    /// <summary>
    /// Add Authorization and MockAuthentication middleware to Application
    /// </summary>
    public static class MockAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseMockAuthentication(this IApplicationBuilder app)
        {
            app.UseMiddleware<MockAuthenticationMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}
