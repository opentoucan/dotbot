using System.Reflection;
using Amazon.S3;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.Infrastructure.GraphQL;
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
        builder.AddGraphQL();
        return builder;
    }

    private static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDbDefaults();
        builder.Services.AddMongoDbCollection<BotCommand>();
        builder.Services.AddMongoDbCollection<DiscordServer>();
        builder.Services.AddScoped<DbContext>(c =>
            new DbContext(c.GetRequiredService<MongoDbContext>(), c.GetRequiredService<IMongoCollection<BotCommand>>(), c.GetRequiredService<IMongoCollection<DiscordServer>>()));
        return builder;
    }

    private static IHostApplicationBuilder AddGraphQL(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddGraphQL()
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMongoDbSorting()
            .AddMongoDbFiltering()
            .AddMongoDbProjections()
            .AddMongoDbPagingProviders();
        
        return builder;
    }
}