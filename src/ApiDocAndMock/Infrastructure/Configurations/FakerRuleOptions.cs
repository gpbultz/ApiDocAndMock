using Bogus;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class FakerRuleOptions
    {
        public Action<Dictionary<string, Func<Faker, object>>> Configure { get; set; }
    }

}
