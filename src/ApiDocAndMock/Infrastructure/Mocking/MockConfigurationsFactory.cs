using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public static class MockConfigurationsFactory
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<Faker, object>>> _configurations = new();

        public static void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class
        {
            var builder = new MockConfigurationBuilder<T>();
            configure(builder);
            _configurations[typeof(T)] = builder.Build();
        }

        public static Dictionary<string, Func<Faker, object>> TryGetConfigurations<T>() where T : class
        {
            return _configurations.TryGetValue(typeof(T), out var config) ? config : null;
        }
    }


}
