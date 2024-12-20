using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class MockingConfigurations
    {
        private readonly Dictionary<Type, PropertyConfiguration> _configurations = new();

        public void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class
        {
            var builder = new MockConfigurationBuilder<T>();
            configure(builder);

            var propertyConfig = new PropertyConfiguration
            {
                FlatConfigurations = builder.FlatConfigurations,
                NestedConfigurations = builder.NestedConfigurations
            };

            _configurations[typeof(T)] = propertyConfig;
        }

        public PropertyConfiguration GetConfigurationFor<T>() where T : class
        {
            if (_configurations.TryGetValue(typeof(T), out var config))
            {
                return config;
            }
            throw new InvalidOperationException($"No mock configuration found for type {typeof(T).Name}");
        }

        public bool TryGetConfigurationFor<T>(out PropertyConfiguration config) where T : class
        {
            return _configurations.TryGetValue(typeof(T), out config);
        }
    }


}
