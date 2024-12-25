using ApiDocAndMock.Application.Interfaces;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class MockConfigurationBuilder<T> where T : class
    {
        private readonly Dictionary<string, Func<Faker, object>> _propertyConfigurations = new();
        private readonly IServiceProvider _serviceProvider;  // Inject IServiceProvider

        public MockConfigurationBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MockConfigurationBuilder<T> ForProperty(string propertyName, Func<Faker, object> generator)
        {
            _propertyConfigurations[propertyName] = generator;
            return this;
        }

        public MockConfigurationBuilder<T> ForPropertyObject<TNested>(string propertyName) where TNested : class, new()
        {
            _propertyConfigurations[propertyName] = faker =>
            {
                // Resolve IApiMockDataFactory lazily
                var mockDataFactory = _serviceProvider.GetRequiredService<IApiMockDataFactory>();
                return mockDataFactory.CreateMockObject<TNested>();
            };
            return this;
        }

        public MockConfigurationBuilder<T> ForPropertyObjectList<TNested>(string propertyName, int count = 5) where TNested : class, new()
        {
            _propertyConfigurations[propertyName] = faker =>
            {
                var mockDataFactory = _serviceProvider.GetRequiredService<IApiMockDataFactory>();
                return mockDataFactory.CreateMockObjects<TNested>(count);
            };
            return this;
        }

        public Dictionary<string, Func<Faker, object>> Build()
        {
            return _propertyConfigurations;
        }
    }
}

