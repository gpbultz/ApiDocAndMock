using ApiDocAndMock.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class MockingConfigurationOptions
    {
        public Action<IMockConfigurationsFactory>? Configure { get; set; }
    }
}
