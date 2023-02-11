using Dotbot.Database.Repositories;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Common.Services;

public class PersistentSettingsService : IPersistentSettingsService
{
    private readonly IPersistentSettingsRepository _repository;

    public PersistentSettingsService(IPersistentSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<T>> GetSetting<T>(string key)
    {
        var setting = await _repository.GetSetting(key);
        if (setting == null) return Fail($"No value for setting {key}");

        var value = setting.Get<T>();

        return value != null ? Ok(value) : Fail($"Invalid type for setting {key}");
    }

    public async Task<Result> SetSetting(string key, object value)
    {
        await _repository.SaveOrUpdateSetting(key, value);
        return Ok();
    }
}