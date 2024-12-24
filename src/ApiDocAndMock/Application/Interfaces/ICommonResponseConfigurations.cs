using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface ICommonResponseConfigurations
    {
        void RegisterResponseExample(int statusCode, ProblemDetails details);

        ProblemDetails? GetProblemDetailsForStatusCode(int statusCode);
    }
}
