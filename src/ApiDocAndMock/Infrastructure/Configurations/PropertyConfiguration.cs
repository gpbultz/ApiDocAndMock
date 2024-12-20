using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class PropertyConfiguration
    {
        public Dictionary<string, Func<Faker, object>> FlatConfigurations { get; set; }
        public Dictionary<string, (Func<Faker, object> Generator, Type NestedType)> NestedConfigurations { get; set; }
    }

}
