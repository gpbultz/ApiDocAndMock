using ApiDocAndMock.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    /// <summary>
    /// Add Authorization and MockAuthentication middleware to Application
    /// </summary>
    public static class MockAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseMockAuthentication(this IApplicationBuilder app)
        {
            // Ensure routing is set up
            app.UseRouting();

            app.UseWhen(
                context => !context.Request.Path.StartsWithSegments("/swagger") &&
                !context.Request.Path.StartsWithSegments("/token"), appBuilder =>
            {
                appBuilder.UseMiddleware<MockAuthenticationMiddleware>();

                appBuilder.UseAuthentication();
                appBuilder.UseAuthorization();
            });



            return app;
        }
    }
}
