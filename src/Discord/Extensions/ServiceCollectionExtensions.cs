using Discord.Application.BotCommandHandlers;
using Discord.Application.BotCommands;
using Discord.Application.Factories;
using Discord.Application.Repositories;
using Discord.Application.Services;
using Discord.Discord.EventHandlers;
using Discord.Discord.InteractionHandler;
using Discord.Interactions;
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
        services.AddSingleton<ButtonActionEventHandler>();
        services.AddHttpClient<SaveBotCommandHandler>();
        
        //TODO: .AddImplementingInterfaces
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IDiscordServerRepository, DiscordServerRepository>();
        services.AddSingleton<IBotCommandCreatorFactory, BotCommandCreatorFactory>();
        return services;
    }
}