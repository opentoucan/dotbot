using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Xkcd.API.Entities;

namespace Xkcd.API.Extensions;

public static class DependencyConfigurationExtensions
{
    public static IServiceCollection AddMongoDbCollection<T>(this IServiceCollection services)
        where T : class
    {
        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            BsonClassMap.TryRegisterClassMap<Entity>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
            });
            
            BsonClassMap.TryRegisterClassMap<Entities.Xkcd>(cm =>
            {
                BsonSerializer.RegisterSerializer(typeof(DateTimeOffset),
                    new DateTimeOffsetSerializer(BsonType.DateTime));
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(GuidGenerator.Instance).SetIgnoreIfDefault(true);
            });

            var database = provider.GetRequiredService<IMongoDatabase>();

            return database.GetCollection<T>(typeof(T).Name);
        }

        services.TryAddSingleton(MongoDbCollectionFactory);


        return services;
    }
}