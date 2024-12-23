using ApiDocAndMock.Infrastructure.Mocking;
using Bogus;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class MockConfigurationBuilder<T> where T : class
    {
        private readonly Dictionary<string, Func<Faker, object>> _propertyConfigurations = new();

        public MockConfigurationBuilder<T> ForProperty(string propertyName, Func<Faker, object> generator)
        {
            _propertyConfigurations[propertyName] = generator;
            return this;
        }

        // For a single nested object
        public MockConfigurationBuilder<T> ForPropertyObject<TNested>(string propertyName) where TNested : class, new()
        {
            _propertyConfigurations[propertyName] = faker => ApiMockDataFactoryStatic.CreateMockObject<TNested>();
            return this;
        }

        // For a list of nested objects
        public MockConfigurationBuilder<T> ForPropertyObjectList<TNested>(string propertyName, int count = 5) where TNested : class, new()
        {
            _propertyConfigurations[propertyName] = faker => ApiMockDataFactoryStatic.CreateMockObjects<TNested>(count);
            return this;
        }

        public Dictionary<string, Func<Faker, object>> Build()
        {
            return _propertyConfigurations;
        }
    }
}

