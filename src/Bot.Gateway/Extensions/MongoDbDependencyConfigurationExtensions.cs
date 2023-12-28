using Bot.Gateway.Infrastructure.Entities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Bot.Gateway.Extensions;

public static class MongoDbDependencyConfigurationExtensions
{
    public static IServiceCollection AddMongoDbCollection<T>(this IServiceCollection services)
        where T : class
    {
        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            var database = provider.GetRequiredService<IMongoDatabase>();

            BsonSerializer.RegisterIdGenerator(
                typeof(Guid),
                GuidGenerator.Instance
            );

            BsonClassMap.TryRegisterClassMap<Entity>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
                cm.MapIdMember(m => m.Id)
                    .SetSerializer(GuidSerializer.StandardInstance);
            });

            BsonClassMap.TryRegisterClassMap<BotCommand>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(m => m.ServerId);
                cm.MapMember(m => m.Name);
                cm.MapMember(m => m.Content);
                cm.MapMember(m => m.AttachmentIds);
                cm.MapMember(m => m.CreatorId);
                cm.MapMember(m => m.Created);
            });

            return database.GetCollection<T>(typeof(T).Name);
        }

        services.TryAddSingleton(MongoDbCollectionFactory);

        return services;
    }
}