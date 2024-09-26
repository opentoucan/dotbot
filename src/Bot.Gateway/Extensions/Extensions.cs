using System.Net;
using System.Reflection;
using Bot.Gateway.Application.Behaviours;
using Bot.Gateway.Application.Queries;
using Bot.Gateway.Dto.AutoMapper;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
            cfg.AddOpenBehavior(typeof(TransactionBehaviour<,>));
        });
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<ICustomCommandRepository, CustomCommandRepository>();
        builder.Services.AddScoped<ICustomCommandQueries, CustomCommandQueries>();
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
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return builder;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var retryCount = 3;
        var retrySleepDuration = 2;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            //.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
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
            x.AddEntityFrameworkOutbox<DotbotContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
    
            x.UsingRabbitMq((context,cfg) =>
            {
                cfg.Host(rabbitMqSection.GetValue<string>("Endpoint"), 
                    rabbitMqSection.GetValue<ushort>("Port"),
                    "/", 
                    h => {
                        h.Username(rabbitMqSection.GetValue<string>("User")!);
                        h.Password(rabbitMqSection.GetValue<string>("Password")!);
                });
                cfg.UseDelayedMessageScheduler();                
                cfg.ConfigureEndpoints(context);
            });
        });
        return builder;
    }
    
    private static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<DbContext>();
        builder.Services.AddDbContext<DotbotContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("dotbot"));
        });
        return builder;
    }
}