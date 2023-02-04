using Dotbot.Database.Entities;

namespace Dotbot.Database.Repositories;

public interface IPersistentSettingsRepository : IRepository<PersistentSetting>
{
    Task<PersistentSetting?> GetSetting(string key);
    Task SaveOrUpdateSetting(string key, object value);
}