using System.Reflection;
using Discord;
using Discord.WebSocket;
using Dotbot.Database;
using Dotbot.Database.Entities;
using Dotbot.Database.Extensions;
using Dotbot.Database.Settings;
using Dotbot.Discord.Extensions;
using Dotbot.Discord.IntegrationEvents.EventHandlers;
using Dotbot.Discord.InteractionHandler;
using Dotbot.Discord.Settings;
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

// Add services to the container.

//builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();


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
builder.Services.AddMongoDbCollection<ChatServer, ChatServerClassMapExtension>("springGuild",
    new ChatServerClassMapExtension());
builder.Services.AddMongoDbCollection<BotCommand, DiscordCommandClassMapExtension>("DiscordCommands",
    new DiscordCommandClassMapExtension());
builder.Services.AddMongoDbCollection<PersistentSetting, PersistentSettingClassMapExtension>("PersistentSettings",
    new PersistentSettingClassMapExtension());

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));

builder.Services.AddSingleton(discordConfig);

builder.Services.AddServices();

builder.Services.AddSingleton<DbContext>(c =>
    new DbContext(c.GetRequiredService<IMongoCollection<BotCommand>>(),
        c.GetRequiredService<IMongoCollection<ChatServer>>(),
        c.GetRequiredService<IMongoCollection<PersistentSetting>>()));

builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

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
await client.LoginAsync(TokenType.Bot, builder.Configuration["Discord:BotToken"]);
await client.StartAsync();
await client.SetGameAsync(builder.Configuration["Discord:GameStatus"]);
await app.RunAsync();