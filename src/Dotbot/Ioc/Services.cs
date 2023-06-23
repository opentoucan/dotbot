
using Dotbot.Infrastructure.Repositories;
using Dotbot.Services;

namespace Dotbot.Ioc;

public static class Services
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {

        services.AddScoped<IBotCommandRepository, BotCommandRepository>();
        services.AddScoped<IXkcdCommandService, XkcdCommandService>();
        return services;
    }
}