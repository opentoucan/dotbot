using Dotbot.Api.Application.Queries;
using Dotbot.Api.Queries;
using Dotbot.Api.Services;
using Dotbot.Infrastructure.Extensions;
using Dotbot.Infrastructure.Repositories;
using ServiceDefaults;

namespace Dotbot.Api.Extensions;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.ConfigureXkcd();
        builder.ConfigureMot();
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IGuildRepository, GuildRepository>();
        builder.Services.AddScoped<IGuildQueries, GuildQueries>();
        builder.Services.AddScoped<ICustomCommandService, CustomCommandService>();
        builder.AddDatabase();
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
}