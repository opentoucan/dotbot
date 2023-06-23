using Discord.Interactions;
using Discord.WebSocket;
using Dotbot.Discord.CommandHandlers;
using Dotbot.Discord.CommandHandlers.Moderator;
using Dotbot.Discord.EventListeners;
using Dotbot.Discord.Factories;
using Dotbot.Discord.Repositories;
using Dotbot.Discord.Services;

namespace Dotbot.Discord.Extensions;

public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<InteractionHandler.InteractionHandler>();
        services.AddSingleton<MessageReceivedEventListener>();
        services.AddHttpClient<SaveBotCommandHandler>();
        
        //TODO: .AddImplementingInterfaces
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<BotCommandHandler, DefaultBotCommandHandler>();
        services.AddTransient<BotCommandHandler, PingBotCommandHandler>();
        services.AddTransient<BotCommandHandler, SaveBotCommandHandler>();
        services.AddTransient<BotCommandHandler, SavedCommandHandler>();
        services.AddTransient<BotCommandHandler, AvatarCommandHandler>();
        services.AddTransient<BotCommandHandler, XkcdBotCommandHandler>();
        services.AddTransient<BotCommandHandler, AddModeratorCommandHandler>();
        services.AddTransient<BotCommandHandler, RemoveModeratorCommandHandler>();
        services.AddTransient<BotCommandHandler, SetXkcdChannelCommandHandler>();
        services.AddTransient<BotCommandHandler, InfoCommandHandler>();
        services.AddTransient<BotCommandHandler, SearchCommandHandler>();
        services.AddTransient<BotCommandHandler, PlayMusicHandler>();
        services.AddTransient<BotCommandHandler, SkipMusicHandler>();
        services.AddTransient<BotCommandHandler, TrackInfoHandler>();
        services.AddSingleton<IBotCommandHandlerFactory, BotCommandHandlerFactory>();
        services.AddTransient<IDiscordServerRepository, DiscordServerRepository>();
        return services;
    }
}