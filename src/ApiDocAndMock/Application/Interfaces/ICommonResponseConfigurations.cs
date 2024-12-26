using Microsoft.AspNetCore.Mvc;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface ICommonResponseConfigurations
    {
        void RegisterResponseExample(int statusCode, ProblemDetails details);

        ProblemDetails? GetProblemDetailsForStatusCode(int statusCode);
    }
}
