using System.Diagnostics;
using Dotbot.Discord.Entities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Dotbot.Discord.Extensions;

public static class MongoDbDependencyConfigurationExtensions
{
    public static IServiceCollection AddMongoDbCollection<T>(this IServiceCollection services)
        where T : class
    {
        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            try
            {
                BsonClassMap.TryRegisterClassMap<Entity>(cme =>
                {
                    cme.SetIsRootClass(true);
                    cme.AutoMap();
                });

                BsonClassMap.TryRegisterClassMap<DiscordServer>(cm =>
                {
                    cm.MapIdMember(c => c.Id).SetIdGenerator(GuidGenerator.Instance).SetIgnoreIfDefault(true);
                    cm.SetIgnoreExtraElements(true);
                    cm.MapMember(m => m.Server)
                        .SetElementName("guildId");
                    cm.MapMember(m => m.UserWordCounts)
                        .SetElementName("userWordCounts");
                    cm.MapMember(m => m.ModeratorIds)
                        .SetElementName("privilegedUsers");
                    cm.MapMember(m => m.MemeChannelIds)
                        .SetElementName("memeChannels");
                    cm.MapMember(m => m.DeafenedChannelIds)
                        .SetElementName("deafenedChannels");
                    cm.MapMember(m => m.XkcdChannelId)
                        .SetElementName("xkcdChannelId");
                    cm.MapMember(m => m.Volume)
                        .SetElementName("volume");
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