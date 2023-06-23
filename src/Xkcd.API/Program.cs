using Grpc.Net.Client;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using Quartz;
using Xkcd.API;
using Xkcd.API.Config;
using Xkcd.API.Entities;
using Xkcd.API.Extensions;
using Xkcd.API.Infrastructure;
using Xkcd.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    var jobKey = new JobKey(nameof(XkcdJob));
    q.AddJob<XkcdJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity($"{nameof(XkcdJob)}-trigger")
        .WithCronSchedule(builder.Configuration.GetValue<string>("CronSchedule")!));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddGrpc();
builder.Services.AddGrpcServices();
builder.Services.AddGrpcClient<XkcdApi.XkcdService.XkcdServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5001");
});

builder.Services.AddHttpClient<XkcdService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl"));
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
            retryAttempt)));
}

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetValue<string>("MongoSettings:ConnectionString")));
builder.Services.AddSingleton<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase("Xkcd"));

builder.Services.AddMongoDbCollection<Xkcd.API.Entities.Xkcd>();
builder.Services.AddMongoDbCollection<Entity>();
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

builder.Services.AddScoped<IXkcdContext, XkcdContext>();

var app = builder.Build();

app.MapGrpcService<XkcdService>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();