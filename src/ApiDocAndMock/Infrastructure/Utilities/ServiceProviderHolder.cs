using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Utilities
{
    public static class ServiceProviderHolder
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public static void Initialize(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }
    }
}
