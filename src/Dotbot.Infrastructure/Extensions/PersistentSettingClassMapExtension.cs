using Dotbot.Database.Entities;
using MongoDB.Bson.Serialization;

namespace Dotbot.Database.Extensions;

public class PersistentSettingClassMapExtension: IClassMapExtension
{
    public BsonClassMap Register()
    {
        return BsonClassMap.RegisterClassMap<PersistentSetting>(cm =>
        {
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(m => m.Key)
                .SetElementName("key");
            cm.MapMember(m => m.Value)
                .SetElementName("value");
        });
    }
}