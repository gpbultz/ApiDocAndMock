using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Utilities;
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

        public MockConfigurationBuilder<T> ForPropertyObject<TNested>(string propertyName) where TNested : class, new()
        {
            _propertyConfigurations[propertyName] = faker =>
            {
                var mockDataFactory = ServiceProviderHelper.GetService<IApiMockDataFactory>();
                return mockDataFactory.CreateMockObject<TNested>();
            };
            return this;
        }

        public MockConfigurationBuilder<T> ForPropertyObjectList<TNested>(string propertyName, int count = 5) where TNested : class, new()
        {
            _propertyConfigurations[propertyName] = faker =>
            {
                var mockDataFactory = ServiceProviderHelper.GetService<IApiMockDataFactory>();
                return mockDataFactory.CreateMockObjects<TNested>(count);
            };
            return this;
        }

        public MockConfigurationBuilder<T> ForPropertyTuple<T1, T2>(string propertyName, Func<Faker, T1> item1Rule, Func<Faker, T2> item2Rule)
        {
            _propertyConfigurations[propertyName] = faker =>
            {
                var item1 = item1Rule(faker);
                var item2 = item2Rule(faker);
                return Tuple.Create(item1, item2);
            };

            return this;
        }

        public MockConfigurationBuilder<T> ForPropertyDictionary<TKey, TValue>(string propertyName, int count, Func<Faker, TKey> keyRule, Func<Faker, TValue> valueRule)
        {
            _propertyConfigurations[propertyName] = faker =>
            {
                var dict = new Dictionary<TKey, TValue>();

                for (int i = 0; i < count; i++)
                {
                    dict[keyRule(faker)] = valueRule(faker);
                }

                return dict;
            };

            return this;
        }

        public Dictionary<string, Func<Faker, object>> Build()
        {
            return _propertyConfigurations;
        }
    }
}

