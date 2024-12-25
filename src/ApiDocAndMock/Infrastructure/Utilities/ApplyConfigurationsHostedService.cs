using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Utilities
{
    public class ApplyConfigurationsHostedService : IHostedService
    {
        private readonly IServiceProvider _provider;

        public ApplyConfigurationsHostedService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _provider.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IMockConfigurationsFactory>();

                // Apply Mocking Configurations
                var mockingOptions = scope.ServiceProvider.GetRequiredService<IOptions<MockingConfigurationOptions>>().Value;
                mockingOptions.Configure?.Invoke(factory);

                // Apply Faker Rules
                var fakerOptions = scope.ServiceProvider.GetRequiredService<IOptions<FakerRuleOptions>>().Value;
                var rules = factory.TryGetConfigurations<object>();  // Get default faker rules
                fakerOptions.Configure?.Invoke(rules);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
