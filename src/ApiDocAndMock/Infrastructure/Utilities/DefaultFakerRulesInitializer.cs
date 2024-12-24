using ApiDocAndMock.Application.Interfaces;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Utilities
{
    public class DefaultFakerRulesInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Func<Faker, object>> _defaultRules;

        public DefaultFakerRulesInitializer(IServiceProvider serviceProvider,
            Dictionary<string, Func<Faker, object>> defaultRules)
        {
            _serviceProvider = serviceProvider;
            _defaultRules = defaultRules;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Resolve IApiMockDataFactory safely after DI container is built
            var mockDataFactory = _serviceProvider.GetRequiredService<IApiMockDataFactory>();

            foreach (var rule in _defaultRules)
            {
                mockDataFactory.AddDefaultFakerRule(rule.Key, rule.Value);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
