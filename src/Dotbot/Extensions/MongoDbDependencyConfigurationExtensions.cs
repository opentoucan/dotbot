using System.Diagnostics;
using Dotbot.Models;
using Dotbot.SeedWork;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Dotbot.Extensions;

public static class MongoDbDependencyConfigurationExtensions
{
    public static IServiceCollection AddMongoDbCollection<T>(this IServiceCollection services)
        where T : class
    {
        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            try
            {
                BsonClassMap.TryRegisterClassMap<Entity>(cm =>
                {
                    cm.SetIsRootClass(true);
                    cm.AutoMap();
                });

                BsonClassMap.TryRegisterClassMap<Enumeration>(cm =>
                {
                    cm.SetIsRootClass(true);
                    cm.MapMember(m => m.Id);
                    cm.MapMember(m => m.Name);
                });

                BsonClassMap.TryRegisterClassMap<BotCommandType>(cm =>
                {
                    cm.MapCreator(c => new BotCommandType(c.Id, c.Name));
                });

                BsonClassMap.TryRegisterClassMap<BotCommand>(cm =>
                {
                    cm.MapIdMember(c => c.Id).SetIdGenerator(GuidGenerator.Instance).SetIgnoreIfDefault(true);
                    cm.SetIgnoreExtraElements(true);
                    cm.MapMember(m => m.ServiceId)
                        .SetElementName("guildId");
                    cm.MapMember(m => m.Name)
                        .SetElementName("key");
                    cm.MapMember(m => m.Content)
                        .SetElementName("content");
                    cm.MapMember(m => m.Type)
                        .SetElementName("type");
                    cm.MapMember(m => m.CreatorId)
                        .SetElementName("creatorId");
                    cm.MapMember(m => m.Created)
                        .SetElementName("created");
                });
                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            var database = provider.GetRequiredService<IMongoDatabase>();

            return database.GetCollection<T>(typeof(T).Name);
        }

        services.TryAddSingleton(MongoDbCollectionFactory);

        return services;
    }
}