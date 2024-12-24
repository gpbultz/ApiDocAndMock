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
    public class ApplyMockConfigurationsHostedService : IHostedService
    {
        private readonly IServiceProvider _provider;

        public ApplyMockConfigurationsHostedService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _provider.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IMockConfigurationsFactory>();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<MockingConfigurationOptions>>().Value;

                options.Configure?.Invoke(factory);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
