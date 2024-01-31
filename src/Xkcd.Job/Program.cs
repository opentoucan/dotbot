using System.Reflection;
using MassTransit;
using MassTransit.MongoDbIntegration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using Xkcd.Job.Behaviours;
using Xkcd.Job.Commands;
using Xkcd.Job.Config;
using Xkcd.Job.Extensions;
using Xkcd.Job.Infrastructure;
using Xkcd.Sdk;

var builder = Host.CreateApplicationBuilder(args);
builder.Environment.ContentRootPath = Directory.GetCurrentDirectory();


builder.Services.AddLogging();
builder.Services.AddHttpClient<XkcdService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl")!);
    })
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .RetryAsync(3);
}

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")));
builder.Services.AddSingleton<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));

builder.Services.AddMongoDbCollection<Xkcd.Job.Infrastructure.Entities.Xkcd>();
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

builder.Services.AddScoped<DbContext>(c =>
    new DbContext(c.GetRequiredService<MongoDbContext>(), c.GetRequiredService<IMongoCollection<Xkcd.Job.Infrastructure.Entities.Xkcd>>()));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(TransactionBehaviour<,>));
});


using IHost host = builder.Build();
host.Start();
var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
var client = host.Services.GetRequiredService<XkcdService>();
var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();

logger.LogInformation("Checking for new XKCD comic");

var existingXkcd = dbContext.XkcdLatest.AsQueryable().OrderBy(x => x.ComicNumber).FirstOrDefault();

var latestXkcd = await client.GetXkcdComicAsync(null, CancellationToken.None);

if (latestXkcd == null)
{
    logger.LogError("Failed to get latest XKCD comic");
    return;
}

if (existingXkcd?.ComicNumber >= latestXkcd.ComicNumber)
{
    logger.LogInformation("Retrieved comic is not newer than existing comic: {}", existingXkcd.ComicNumber);
    return;
}

logger.LogInformation("Current comic is {}, last checked was {}",  existingXkcd?.ComicNumber, latestXkcd.ComicNumber);

var setXkcdCommand = new SetXkcdCommand(latestXkcd.ComicNumber, latestXkcd.ImageUrl, latestXkcd.Title, latestXkcd.AltText, latestXkcd.DatePosted);
        
await mediatr.Send(setXkcdCommand);

lifetime.StopApplication();
await host.WaitForShutdownAsync();