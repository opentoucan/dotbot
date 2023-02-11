using FluentResults;

namespace Dotbot.Common.Services;

public interface IPersistentSettingsService
{
    Task<Result<T>> GetSetting<T>(string key);
    Task<Result> SetSetting(string key, object value);
}