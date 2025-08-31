using System.Reflection;
using Dotbot.Api.Application.Queries;
using Dotbot.Api.Queries;
using Dotbot.Api.Services;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Behaviours;
using Dotbot.Infrastructure.Extensions;
using Dotbot.Infrastructure.Repositories;
using MassTransit;
using ServiceDefaults;

namespace Dotbot.Api.Extensions;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(TransactionBehaviour<,>));
        });
        builder.ConfigureXkcd();
        builder.ConfigureMot();
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IGuildRepository, GuildRepository>();
        builder.Services.AddScoped<IGuildQueries, GuildQueries>();
        builder.Services.AddScoped<ICustomCommandService, CustomCommandService>();
        builder.AddDatabase();
        builder.AddMassTransit();
        builder.ConfigureAWS();
        return builder;
    }

    public static IHostApplicationBuilder ConfigureXkcd(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IXkcdService, XkcdService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl")!);
        })
        .AddStandardResilienceHandler();

        return builder;
    }

    public static IHostApplicationBuilder ConfigureMot(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IMotService, MotService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("MotUrl")!);
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddStandardResilienceHandler(options =>
            {
                var timeSpan = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = timeSpan;
                options.CircuitBreaker.SamplingDuration = timeSpan * 2;
                options.TotalRequestTimeout.Timeout = timeSpan * 3;
            });

        return builder;
    }

    public static IHostApplicationBuilder AddMassTransit(this IHostApplicationBuilder builder)
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

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSection.GetValue<string>("Endpoint"),
                    rabbitMqSection.GetValue<ushort>("Port"),
                    "/",
                    h =>
                    {
                        h.Username(rabbitMqSection.GetValue<string>("User")!);
                        h.Password(rabbitMqSection.GetValue<string>("Password")!);
                    });
                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(context);
            });
        });
        return builder;
    }
}