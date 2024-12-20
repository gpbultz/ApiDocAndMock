using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class MockConfigurationBuilder<T> where T : class
    {
        internal Dictionary<string, Func<Faker, object>> FlatConfigurations { get; } = new();
        internal Dictionary<string, (Func<Faker, object> Generator, Type NestedType)> NestedConfigurations { get; } = new();

        public MockConfigurationBuilder<T> ForProperty(string propertyName, Func<Faker, object> generator)
        {
            FlatConfigurations[propertyName] = generator;
            return this;
        }

        public MockConfigurationBuilder<T> ForNestedProperty<TNested>(string propertyName, Func<Faker, TNested> generator) where TNested : class
        {
            NestedConfigurations[propertyName] = (faker => generator(faker), typeof(TNested));
            return this;
        }
    }
}
