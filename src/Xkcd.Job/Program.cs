using Contracts.MessageBus;
using MassTransit;
using MassTransit.MongoDbIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using Xkcd.Job.Config;
using Xkcd.Job.Extensions;
using Xkcd.Job.Infrastructure;
using Xkcd.Sdk;

var builder = Host.CreateApplicationBuilder(args);

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


using IHost host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
var client = host.Services.GetRequiredService<XkcdService>();
var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

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

await dbContext.BeginTransactionAsync();
var newXkcd = new Xkcd.Job.Infrastructure.Entities.Xkcd(latestXkcd.ComicNumber, latestXkcd.DatePosted);
if (existingXkcd == null)
{
    await dbContext.XkcdLatest.InsertOneAsync(dbContext.Session, newXkcd, cancellationToken: CancellationToken.None);
}
else
{

    newXkcd.Id = existingXkcd.Id;
    await dbContext.XkcdLatest.ReplaceOneAsync(dbContext.Session,
        Builders<Xkcd.Job.Infrastructure.Entities.Xkcd>.Filter.Eq(x => x.ComicNumber, existingXkcd.ComicNumber),
        newXkcd, cancellationToken: CancellationToken.None);
}
        
var xkcdPostedEvent = new XkcdPostedEvent(latestXkcd.ComicNumber, latestXkcd.DatePosted, latestXkcd.AltText, latestXkcd.ImageUrl, latestXkcd.Title);
await publishEndpoint.Publish(xkcdPostedEvent, CancellationToken.None);
await dbContext.CommitTransactionAsync();
