using Discord.Application.Repositories;
using Discord.BotCommandHandlers;
using Discord.BotCommandHandlers.Moderator;
using Discord.Discord.InteractionHandler;
using Discord.EventHandlers;
using Discord.Factories;
using Discord.Interactions;
using Discord.Services;
using Discord.WebSocket;

namespace Discord.Extensions;

public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<InteractionHandler>();
        services.AddSingleton<MessageReceivedEventHandler>();
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