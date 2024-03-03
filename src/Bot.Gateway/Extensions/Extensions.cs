using System.Reflection;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Services;
using MassTransit.MongoDbIntegration;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using ServiceDefaults;
using Xkcd.Sdk;

namespace Bot.Gateway.Extensions;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        builder.Services.AddSingleton<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IBotCommandRepository, BotCommandRepository>();
        builder.Services.AddHttpClient<XkcdService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl")!);
            })
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(3));

        builder.AddDatabase();
        builder.AddMassTransit();
        builder.ConfigureDiscordServices();
        builder.ConfigureAWS();
        builder.AddDiscordInteractionAuth();
        return builder;
    }

    private static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDbDefaults();
        builder.Services.AddMongoDbCollection<BotCommand>();
        builder.Services.AddScoped<DbContext>(c =>
            new DbContext(c.GetRequiredService<MongoDbContext>(), c.GetRequiredService<IMongoCollection<BotCommand>>()));
        return builder;
    }
}