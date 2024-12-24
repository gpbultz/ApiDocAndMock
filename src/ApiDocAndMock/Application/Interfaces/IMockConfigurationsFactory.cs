using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface IMockConfigurationsFactory
    {
        void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class;
        Dictionary<string, Func<Faker, object>> TryGetConfigurations<T>() where T : class;
    }
}
