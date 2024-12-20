using ApiDocAndMock.Infrastructure.Mocking;
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

        public MockConfigurationBuilder<T> ForNestedProperty<TNested>(string propertyName, int nestedCount = 5) where TNested : class, new()
        {
            NestedConfigurations[propertyName] = (faker =>
            {
                return Enumerable.Range(0, nestedCount)
                                 .Select(_ => ApiMockDataFactory.CreateMockObject<TNested>(nestedCount - 1))
                                 .ToList();
            }, typeof(List<TNested>));

            return this;
        }



    }
}
