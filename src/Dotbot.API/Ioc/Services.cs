using Dotbot.Database.Repositories;
using Dotbot.Database.Services;

namespace Dotbot.Ioc;

public static class Services
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {

        //TODO: .AddImplementingInterfaces
        services.AddTransient<IFileService, FileService>();

        services.AddTransient<IChatServerRepository, ChatServerRepository>();
        services.AddTransient<IBotCommandRepository, BotCommandRepository>();
        services.AddTransient<IPersistentSettingsRepository, PersistentSettingsRepository>();
        return services;
    }
}