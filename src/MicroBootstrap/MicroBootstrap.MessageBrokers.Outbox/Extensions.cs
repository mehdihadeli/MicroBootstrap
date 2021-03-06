using System;
using MicroBootstrap;
using MicroBootstrap.MessageBrokers.Outbox;
using MicroBootstrap.MessageBrokers.Outbox.Configurators;
using MicroBootstrap.MessageBrokers.Outbox.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.MessageBrokers.Outbox
{
    public static class Extensions
    {
        private const string SectionName = "outbox";
        private const string RegistryName = "messageBrokers.outbox";

        public static IServiceCollection AddMessageOutbox(this IServiceCollection services,
            Action<IMessageOutboxConfigurator> configure = null, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var options = services.GetOptions<OutboxOptions>(sectionName);
            services.AddSingleton(options);
            var configurator = new MessageOutboxConfigurator(services, options);

            if (configure is null)
            {
                configurator.AddInMemory();
            }
            else
            {
                configure(configurator);
            }

            if (!options.Enabled)
            {
                return services;
            }
            //adding a background service for handling outbox process
            services.AddHostedService<OutboxProcessor>();

            return services;
        }

        public static IMessageOutboxConfigurator AddInMemory(this IMessageOutboxConfigurator configurator,
            string mongoSectionName = null)
        {
            configurator.Services.AddTransient<IMessageOutbox, InMemoryMessageOutbox>();

            return configurator;
        }
    }
}