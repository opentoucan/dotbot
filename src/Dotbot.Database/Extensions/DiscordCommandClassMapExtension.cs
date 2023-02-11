using Dotbot.Database.Entities;
using MongoDB.Bson.Serialization;

namespace Dotbot.Database.Extensions;

public class DiscordCommandClassMapExtension : IClassMapExtension
{
    public BsonClassMap Register()
    {
        return BsonClassMap.RegisterClassMap<BotCommand>(cm =>
        {
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(m => m.ServiceId)
                .SetElementName("guildId");
            cm.MapMember(m => m.Key)
                .SetElementName("key");
            cm.MapMember(m => m.Content)
                .SetElementName("content");
            cm.MapMember(m => m.FileName)
                .SetElementName("fileName");
            cm.MapMember(m => m.Type)
                .SetElementName("type");
            cm.MapMember(m => m.CreatorId)
                .SetElementName("creatorId");
            cm.MapMember(m => m.Created)
                .SetElementName("created");
        });
    }
}