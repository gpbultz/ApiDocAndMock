using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public class MockConfigurationsFactory : IMockConfigurationsFactory
    {
        private readonly Dictionary<Type, Dictionary<string, Func<Faker, object>>> _configurations = new();
        private readonly Dictionary<string, Func<Faker, object>> _defaultFakerRules = new();

        private readonly IServiceProvider _serviceProvider;

        public MockConfigurationsFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _defaultFakerRules = new()
            {
                ["Name"] = faker => faker.Name.FullName(),
                ["Email"] = faker => faker.Internet.Email(),
                ["Phone"] = faker => faker.Phone.PhoneNumber(),
                ["Address"] = faker => faker.Address.FullAddress(),
                ["City"] = faker => faker.Address.City(),
                ["Region"] = faker => faker.Address.StateAbbr(),
                ["PostalCode"] = faker => faker.Address.ZipCode(),
                ["Country"] = faker => faker.Address.Country()
            };
        }

        public void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class
        {
            var builder = new MockConfigurationBuilder<T>(_serviceProvider);
            configure(builder);
            _configurations[typeof(T)] = builder.Build();
        }

        public void AddDefaultFakerRule(string property, Func<Faker, object> generator)
        {
            _defaultFakerRules[property] = generator;
        }

        public Dictionary<string, Func<Faker, object>> TryGetConfigurations<T>() where T : class
        {
            var mergedRules = new Dictionary<string, Func<Faker, object>>(_defaultFakerRules);

            if (_configurations.TryGetValue(typeof(T), out var rules))
            {
                foreach (var rule in rules)
                {
                    mergedRules[rule.Key] = rule.Value;
                }
            }

            return mergedRules;
        }
    }

}
