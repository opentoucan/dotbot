using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Xkcd.Job.Infrastructure.Entities;

namespace Xkcd.Job.Extensions;

public static class DependencyConfigurationExtensions
{
    public static IServiceCollection AddMongoDbCollection<T>(this IServiceCollection services)
        where T : class
    {
        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            BsonSerializer.RegisterIdGenerator(
                typeof(Guid),
                GuidGenerator.Instance
            );

            BsonClassMap.TryRegisterClassMap<Entity>(cm =>
            {
                cm.SetIsRootClass(true);
                cm.AutoMap();
                cm.MapIdMember(x => x.Id)
                    .SetSerializer(GuidSerializer.StandardInstance);
            });
            
            BsonClassMap.TryRegisterClassMap<Infrastructure.Entities.Xkcd>(cm =>
            {
                BsonSerializer.RegisterSerializer(typeof(DateTimeOffset),
                    new DateTimeOffsetSerializer(BsonType.DateTime));
                cm.SetIgnoreExtraElements(true);
                cm.MapMember(m => m.ComicNumber);
                cm.MapMember(m => m.Posted);
            });

            var database = provider.GetRequiredService<IMongoDatabase>();

            return database.GetCollection<T>(typeof(T).Name);
        }

        services.TryAddSingleton(MongoDbCollectionFactory);


        return services;
    }
}