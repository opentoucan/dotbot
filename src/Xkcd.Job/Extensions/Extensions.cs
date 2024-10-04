using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Xkcd.Job.Behaviours;
using Xkcd.Job.Infrastructure;
using Xkcd.Job.Infrastructure.Repositories;
using Xkcd.Sdk;

namespace Xkcd.Job.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IXkcdRepository, XkcdRepository>();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(Program));
            cfg.AddOpenBehavior(typeof(TransactionBehaviour<,>));
        });
        return builder;
    }
    
    public static IHostApplicationBuilder AddMassTransit(this IHostApplicationBuilder builder)
    {
        var rabbitMqSection = builder.Configuration.GetSection("RabbitMQ");
        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumers(Assembly.GetExecutingAssembly());
            
            x.AddEntityFrameworkOutbox<XkcdContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
    
            x.UsingRabbitMq((context,cfg) =>
            {
                cfg.Host(rabbitMqSection.GetValue<string>("Endpoint"),  h => {
                    h.Username(rabbitMqSection.GetValue<string>("User")!);
                    h.Password(rabbitMqSection.GetValue<string>("Password")!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

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
        builder.Services.AddScoped<DbContext>();
        builder.Services.AddDbContext<XkcdContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("xkcd"));
        });
        return builder;
    }
}