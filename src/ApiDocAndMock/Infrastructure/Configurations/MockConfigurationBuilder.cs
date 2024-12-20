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

        public Dictionary<string, Func<Faker, object>> Build()
        {
            return _propertyConfigurations;
        }
    }
}

