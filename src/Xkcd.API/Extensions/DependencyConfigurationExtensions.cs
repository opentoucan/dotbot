using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Xkcd.API.Entities;
using Xkcd.API.Models;

namespace Xkcd.API.Extensions;

public static class DependencyConfigurationExtensions
{
    public static IServiceCollection AddMongoDbCollection<T>(this IServiceCollection services)
        where T : class
    {
        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            
            if (!BsonClassMap.IsClassMapRegistered(typeof(Entity)))
            {
                BsonClassMap.RegisterClassMap<Entity>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(x => x.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.SetIsRootClass(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Entities.Xkcd)))
            {
                BsonClassMap.RegisterClassMap<Entities.Xkcd>(cm =>
                {
                    BsonSerializer.RegisterSerializer(typeof(DateTimeOffset), new DateTimeOffsetSerializer(BsonType.DateTime));
                    cm.AutoMap();
                });

            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>();
            }

            var database = provider.GetRequiredService<IMongoDatabase>();

            return database.GetCollection<T>(typeof(T).Name);
        }

        services.TryAddSingleton(MongoDbCollectionFactory);


        return services;
    }
}