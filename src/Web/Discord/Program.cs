using System.Reflection;
using Discord;
using Discord.Application.Behaviours;
using Discord.Application.Entities;
using Discord.Application.IntegrationEvents.EventHandlers;
using Discord.Discord.InteractionHandler;
using Discord.WebSocket;
using Discord.Extensions;
using Discord.Settings;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

// Add services to the container.
const string serviceName = "Dotbot.API";
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .AddConsoleExporter()
        .AddSource(serviceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation();
});

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

var section = builder.Configuration.GetSection("MongoDbSettings");
var settings = MongoClientSettings.FromConnectionString(section["ConnectionString"]);
var mongoClient = new MongoClient(settings);

//Discord registrations
var discordConfig = new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers |
                     GatewayIntents.MessageContent | GatewayIntents.GuildVoiceStates,
    AlwaysDownloadUsers = true,
};
//builder.Services.AddSingleton<IMongoClient>(mongoClient);
var db = mongoClient.GetDatabase(section["DatabaseName"]);
builder.Services.AddSingleton(db);
builder.Services.AddSingleton<IGridFSBucket>(new GridFSBucket(db));
builder.Services.AddMongoDbCollection<DiscordServer>();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));

builder.Services
    .AddDotbotClient()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("GraphQLSettings:DotbotUrl")));

builder.Services.AddSingleton(discordConfig);

builder.Services.AddServices();

builder.Services.AddSingleton<DbContext>(c =>
    new DbContext(c.GetRequiredService<IMongoCollection<DiscordServer>>()));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(BotCommandBehaviour<,>));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient("DotbotApiGateway", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration.GetValue<string>("DotbotGatewayUrl"));
});

var rabbitMqConfig = new RabbitMQConfig();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMqConfig);
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqConfig.Endpoint, "/", h =>
        {
            h.Username(rabbitMqConfig.User);
            h.Password(rabbitMqConfig.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
    x.AddConsumer<XkcdPostedEventHandler>();
});

var app = builder.Build();
var client = app.Services.RegisterClientEvents();

await app.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
await client.LoginAsync(TokenType.Bot, builder.Configuration["DiscordSettings:BotToken"]);
await client.StartAsync();
await client.SetGameAsync(builder.Configuration["DiscordSettings:GameStatus"]);
await app.RunAsync();