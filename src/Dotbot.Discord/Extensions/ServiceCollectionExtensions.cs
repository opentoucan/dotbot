using Discord.Interactions;
using Discord.WebSocket;
using Dotbot.Database.Repositories;
using Dotbot.Database.Services;
using Dotbot.Discord.CommandHandlers;
using Dotbot.Discord.CommandHandlers.Moderator;
using Dotbot.Discord.EventListeners;
using Dotbot.Discord.Factories;
using Dotbot.Discord.Services;

namespace Dotbot.Discord.Extensions;

public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<IAudioService, AudioService>();
      //  services.AddSingleton<IPersistentSettingsService, PersistentSettingsService>();
      //  services.AddSingleton<IXkcdService, XkcdService>();
        services.AddSingleton<IChatServerService, ChatServerService>();
        services.AddSingleton<IBotCommandsService, BotCommandsService>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<InteractionHandler.InteractionHandler>();
        services.AddSingleton<MessageReceivedEventListener>();
        services.AddHttpClient<SaveBotCommandHandler>();
     //   services.AddHttpClient<XkcdService>();
        
        //TODO: .AddImplementingInterfaces
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<BotCommandHandler, DefaultBotCommandHandler>();
        services.AddTransient<BotCommandHandler, PingBotCommandHandler>();
        services.AddTransient<BotCommandHandler, SaveBotCommandHandler>();
        services.AddTransient<BotCommandHandler, SavedCommandHandler>();
        services.AddTransient<BotCommandHandler, AvatarCommandHandler>();
     //   services.AddTransient<BotCommandHandler, XkcdBotCommandHandler>();
        services.AddTransient<BotCommandHandler, AddModeratorCommandHandler>();
        services.AddTransient<BotCommandHandler, RemoveModeratorCommandHandler>();
        services.AddTransient<BotCommandHandler, SetXkcdChannelCommandHandler>();
        services.AddTransient<BotCommandHandler, InfoCommandHandler>();
        services.AddTransient<BotCommandHandler, SearchCommandHandler>();
        services.AddTransient<BotCommandHandler, PlayMusicHandler>();
        services.AddTransient<BotCommandHandler, SkipMusicHandler>();
        services.AddTransient<BotCommandHandler, TrackInfoHandler>();
        services.AddSingleton<IBotCommandHandlerFactory, BotCommandHandlerFactory>();
        services.AddTransient<IChatServerRepository, ChatServerRepository>();
        services.AddTransient<IBotCommandRepository, BotCommandRepository>();
        services.AddTransient<IPersistentSettingsRepository, PersistentSettingsRepository>();
        return services;
    }
}