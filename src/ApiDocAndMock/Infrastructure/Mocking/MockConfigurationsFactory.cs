using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public static class MockConfigurationsFactory
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<Faker, object>>> Configurations = new();

        public static void RegisterConfiguration<T>(Dictionary<string, Func<Faker, object>> configuration) where T : class
        {
            Configurations[typeof(T)] = configuration;
        }

        public static Dictionary<string, Func<Faker, object>> GetAllConfigurations<T>() where T : class
        {
            if (Configurations.TryGetValue(typeof(T), out var config))
            {
                return config;
            }

            throw new InvalidOperationException($"No mock configuration found for type {typeof(T).Name}");
        }
    }

}
