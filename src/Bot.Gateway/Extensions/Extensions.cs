using System.Net;
using System.Reflection;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Services;
using MassTransit;
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
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IBotCommandRepository, BotCommandRepository>();
        builder.Services.AddHttpClient<XkcdService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl")!);
            })
            .AddPolicyHandler(GetRetryPolicy());
        
        builder.AddDatabase();
        builder.AddMassTransit();
        builder.ConfigureDiscordServices();
        builder.ConfigureAWS();
        builder.AddDiscordInteractionAuth();
        return builder;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var retryCount = 3;
        var retrySleepDuration = 2;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySleepDuration, retryAttempt)),
                onRetry: (_, _, retryAttempt, _) =>
                {
                    Console.WriteLine($"Retry attempt ({retryAttempt} of {retryCount})");
                });
    }
    private static IHostApplicationBuilder AddMassTransit(this IHostApplicationBuilder builder)
    {
        var rabbitMqSection = builder.Configuration.GetSection("RabbitMQ");
        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumers(Assembly.GetExecutingAssembly());
            x.AddDelayedMessageScheduler();
            x.AddMongoDbOutbox(o =>
            {
                o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());
                o.DatabaseFactory(provider => provider.GetRequiredService<IMongoDatabase>());

                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                o.UseBusOutbox();
            });
    
            x.UsingRabbitMq((context,cfg) =>
            {
                cfg.Host(rabbitMqSection.GetValue<string>("Endpoint"),  h => {
                    h.Username(rabbitMqSection.GetValue<string>("User"));
                    h.Password(rabbitMqSection.GetValue<string>("Password"));
                });
                cfg.UseDelayedMessageScheduler();                
                cfg.ConfigureEndpoints(context);
            });
        });
        return builder;
    }
    
    private static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDbDefaults();
        builder.Services.AddMongoDbCollection<BotCommand>();
        builder.Services.AddScoped<DbContext>();
        return builder;
    }
}