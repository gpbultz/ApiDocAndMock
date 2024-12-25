using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface IMockConfigurationsFactory
    {
        void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class;
        Dictionary<string, Func<Faker, object>> TryGetConfigurations<T>() where T : class;
        void AddDefaultFakerRule(string property, Func<Faker, object> generator);
    }
}
