using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public class MockConfigurationsFactoryWrapper
    {
        public void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class
        {
            MockConfigurationsFactory.RegisterConfiguration(configure);
        }

        public Dictionary<string, Func<Faker, object>>? TryGetConfigurations<T>() where T : class
        {
            return MockConfigurationsFactory.TryGetConfigurations<T>();
        }
    }
}
