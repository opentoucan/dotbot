using MassTransit;
using MassTransit.MongoDbIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using Xkcd.Job.Config;
using Xkcd.Job.Infrastructure;
using Xkcd.Job.Service;
using Xkcd.Sdk;

namespace Xkcd.Job.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IXkcdNotificationService, XkcdNotificationService>();
        return builder;
    }
    public static IHostApplicationBuilder AddMassTransit(this IHostApplicationBuilder builder)
    {
        var rabbitMqConfig = new RabbitMQConfig();
        builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMqConfig);
        builder.Services.AddMassTransit(x =>
        {
            x.AddMongoDbOutbox(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());
                o.DatabaseFactory(provider => provider.GetRequiredService<IMongoDatabase>());

                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                o.UseBusOutbox();
            });
    
            x.UsingRabbitMq((context,cfg) =>
            {
                cfg.Host(rabbitMqConfig.Endpoint,  "/", h => {
                    h.Username(rabbitMqConfig.User);
                    h.Password(rabbitMqConfig.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }

    public static IHostApplicationBuilder AddHttpClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<XkcdService>(client => 
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl")!);
            })
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(3));

        return builder;
    }

    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")));
        builder.Services.AddSingleton<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));
        builder.Services.AddMongoDbCollection<Xkcd.Job.Infrastructure.Entities.Xkcd>();
        builder.Services.AddScoped<DbContext>(c =>
            new DbContext(c.GetRequiredService<MongoDbContext>(), c.GetRequiredService<IMongoCollection<Xkcd.Job.Infrastructure.Entities.Xkcd>>()));
        return builder;
    }
}