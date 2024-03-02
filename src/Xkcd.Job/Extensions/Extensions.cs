using MassTransit;
using MassTransit.MongoDbIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using ServiceDefaults;
using Xkcd.Job.Infrastructure;
using Xkcd.Job.Infrastructure.Repositories;
using Xkcd.Job.Service;
using Xkcd.Sdk;

namespace Xkcd.Job.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IXkcdNotificationService, XkcdNotificationService>();
        builder.Services.AddTransient<IXkcdRepository, XkcdRepository>();
        return builder;
    }
  
    public static IHostApplicationBuilder AddHttpClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IXkcdService, XkcdService>(client => 
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl")!);
            })
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(3));

        return builder;
    }

    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDbDefaults();
        builder.Services.AddMongoDbCollection<Xkcd.Job.Infrastructure.Entities.Xkcd>();
        builder.Services.AddScoped<XkcdContext>(c =>
            new XkcdContext(c.GetRequiredService<MongoDbContext>(), c.GetRequiredService<IMongoCollection<Xkcd.Job.Infrastructure.Entities.Xkcd>>()));
        return builder;
    }
}