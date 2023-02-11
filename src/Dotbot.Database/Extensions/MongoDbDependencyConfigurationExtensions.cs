using System.Diagnostics;
using Dotbot.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Dotbot.Database.Extensions;

public static class MongoDbDependencyConfigurationExtensions
{
    public static IServiceCollection AddMongoDbCollection<T, TM>(this IServiceCollection services, string collectionName, TM classMapExtension)
        where T : class
        where TM : IClassMapExtension
    {
        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            try
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(Entity)))
                {
                    BsonClassMap.RegisterClassMap<Entity>(cme =>
                    {
                        cme.SetIsRootClass(true);
                        cme.AutoMap();
                        cme.MapIdMember(x => x.Id)
                            .SetIgnoreIfDefault(true)
                            .SetIdGenerator(StringObjectIdGenerator.Instance)
                            .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    });
                }
                
                if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                {
                    classMapExtension.Register();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            var database = provider.GetRequiredService<IMongoDatabase>();

            return database.GetCollection<T>(collectionName);
        }

        services.TryAddSingleton(MongoDbCollectionFactory);

        return services;
    }
}