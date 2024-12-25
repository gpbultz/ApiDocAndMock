using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class FakerRuleOptions
    {
        public Action<Dictionary<string, Func<Faker, object>>> Configure { get; set; }
    }

}
