using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

                if (fakerOptions.Configure != null)
                {
                    var rules = new Dictionary<string, Func<Faker, object>>();
                    fakerOptions.Configure(rules);

                    foreach (var rule in rules)
                    {
                        factory.AddDefaultFakerRule(rule.Key, rule.Value);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
