using Dotbot.Database.Entities;
using MongoDB.Driver;

namespace Dotbot.Database.Repositories;

public class PersistentSettingsRepository : IPersistentSettingsRepository
{
    private readonly DbContext _dbContext;

    public PersistentSettingsRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PersistentSetting?> GetSetting(string key)
    {
        return _dbContext.PersistentSettings.AsQueryable().FirstOrDefault(x => x.Key == key);
    }

    public async Task SaveOrUpdateSetting(string key, object value)
    {
        var setting = await GetSetting(key);
        if (setting == null)
        {
            await _dbContext.PersistentSettings.InsertOneAsync(new PersistentSetting{Key = key, Value = value});
        }
        else
        {
            await _dbContext.PersistentSettings.UpdateOneAsync(new FilterDefinitionBuilder<PersistentSetting>().Where(x => x.Key == key),
                new UpdateDefinitionBuilder<PersistentSetting>().Set(nameof(PersistentSetting.Value), value));
        }
    }
}