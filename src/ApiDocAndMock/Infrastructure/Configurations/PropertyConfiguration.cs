using Bogus;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class PropertyConfiguration
    {
        public Dictionary<string, Func<Faker, object>> FlatConfigurations { get; set; }
        public Dictionary<string, (Func<Faker, object> Generator, Type NestedType)> NestedConfigurations { get; set; }
    }

}
