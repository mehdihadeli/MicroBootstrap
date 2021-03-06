using MicroBootstrap.MessageBrokers.Outbox.Messages;
using MicroBootstrap.MessageBrokers.Outbox.Mongo.Internals;
using MicroBootstrap.Mongo;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace MicroBootstrap.MessageBrokers.Outbox.Mongo
{
    public static class Extensions
    {
        public static IMessageOutboxConfigurator AddMongo(this IMessageOutboxConfigurator configurator)
        {
            var services = configurator.Services;
            var options = configurator.Options;

            var inboxCollection = string.IsNullOrWhiteSpace(options.InboxCollection)
                ? "inbox"
                : options.InboxCollection;
            var outboxCollection = string.IsNullOrWhiteSpace(options.OutboxCollection)
                ? "outbox"
                : options.OutboxCollection;

            services.AddMongoRepository<InboxMessage, string>(inboxCollection);
            services.AddMongoRepository<OutboxMessage, string>(outboxCollection);
            services.AddInitializers(typeof(MongoOutboxInitializer));
            services.AddTransient<IMessageOutbox, MongoMessageOutbox>();
            services.AddTransient<IMessageOutboxAccessor, MongoMessageOutbox>();
            services.AddTransient<MongoOutboxInitializer>();

            BsonClassMap.RegisterClassMap<OutboxMessage>(m =>
            {
                m.AutoMap();
                m.UnmapMember(p => p.Message);
                m.UnmapMember(p => p.MessageContext);
            });

            return configurator;
        }
    }
}