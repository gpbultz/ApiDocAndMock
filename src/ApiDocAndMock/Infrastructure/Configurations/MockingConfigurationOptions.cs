using ApiDocAndMock.Application.Interfaces;

namespace ApiDocAndMock.Infrastructure.Configurations
{
    public class MockingConfigurationOptions
    {
        public Action<IMockConfigurationsFactory>? Configure { get; set; }
    }
}
