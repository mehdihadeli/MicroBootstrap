using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MicroBootstrap.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Game.Services.EventProcessor.Tests.Shared.Fixtures
{
    internal sealed class MongoDbFixtureInitializer
    {
        private static int _initialized;
        private readonly bool _seed;
        private readonly IMongoDbSeeder _seeder;

        public MongoDbFixtureInitializer(IMongoDatabase database,
            IMongoDbSeeder seeder,
            MongoDbOptions options)
        {
            _seeder = seeder;
            _seed = options.Seed;
        }

        public Task InitializeAsync()
        {
            if (Interlocked.Exchange(ref _initialized, 1) is 1)
            {
                return Task.CompletedTask;
            }

            RegisterConventions();

            return _seed ? _seeder.SeedAsync() : Task.CompletedTask;
        }

        private void RegisterConventions()
        {
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?),
                new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            ConventionRegistry.Register("conventions", new MongoDbConventions(), x => true);
        }

        private class MongoDbConventions : IConventionPack
        {
            public IEnumerable<IConvention> Conventions => new List<IConvention>
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
        }
    }
}