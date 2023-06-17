using MongoDB.Bson.Serialization;

namespace Dotbot.Database.Extensions;

public interface IClassMapExtension
{
    public BsonClassMap Register();
}