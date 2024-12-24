using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public class MockConfigurationsFactory : IMockConfigurationsFactory
    {
        private readonly Dictionary<Type, Dictionary<string, Func<Faker, object>>> _configurations = new();
        private readonly IServiceProvider _serviceProvider;

        public MockConfigurationsFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class
        {
            var builder = new MockConfigurationBuilder<T>(_serviceProvider);
            configure(builder);
            _configurations[typeof(T)] = builder.Build();
        }

        public Dictionary<string, Func<Faker, object>> TryGetConfigurations<T>() where T : class
        {
            return _configurations.TryGetValue(typeof(T), out var config) ? config : null;
        }
    }

}
