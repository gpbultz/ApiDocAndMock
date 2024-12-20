using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class CommonResponseConfigurations
    {
        private readonly Dictionary<int, ProblemDetails> _responseExamples = new();

        public CommonResponseConfigurations()
        {
            RegisterDefaultExamples();
        }

        public void RegisterResponseExample(int statusCode, ProblemDetails details)
        {
            _responseExamples[statusCode] = details;
        }

        public ProblemDetails? GetProblemDetailsForStatusCode(int statusCode)
        {
            return _responseExamples.TryGetValue(statusCode, out var details) ? details : null;
        }

        private void RegisterDefaultExamples()
        {
            _responseExamples[400] = new ProblemDetails
            {
                Title = "Bad Request",
                Status = 400,
                Detail = "Bad Request - Malformed request syntax or invalid JSON."
            };
            _responseExamples[401] = new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Authentication is required to access this resource."
            };
            _responseExamples[429] = new ProblemDetails
            {
                Title = "Too Many Requests",
                Status = 429,
                Detail = "You have exceeded the allowed number of requests. Please try again later."
            };
            _responseExamples[500] = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected error occurred. Please try again later."
            };
        }
    }

}
