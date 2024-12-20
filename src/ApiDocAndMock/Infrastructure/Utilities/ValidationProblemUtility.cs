using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Utilities
{
    /// <summary>
    /// Utility to return a 422 validation error problem details class. Uses System.ComponentModel.DataAnnotations attributes to determine if something could potentially be validated against
    /// </summary>
    public static class ValidationProblemUtility
    {
        public static ProblemDetails GenerateValidationProblemDetails<T>(T instance)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Validation Error",
                Status = 422,
                Detail = "One or more fields are required.",
            };

            var missingFields = typeof(T)
                .GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                .Where(p => p.GetValue(instance) == null || string.IsNullOrWhiteSpace(p.GetValue(instance)?.ToString()))
                .Select(p => p.Name)
                .ToList();

            if (missingFields.Any())
            {
                problemDetails.Extensions["missingFields"] = missingFields;
            }

            return problemDetails;
        }
    }
}
