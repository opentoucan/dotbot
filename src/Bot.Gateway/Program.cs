using System.Reflection;
using System.Text.Json.Serialization;
using Bot.Gateway.Apis;
using Bot.Gateway.Extensions;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.Infrastructure.GraphQL;
using Bot.Gateway.Settings;
using Discord;
using Discord.Rest;
using MassTransit;
using MassTransit.MongoDbIntegration;
using Microsoft.AspNetCore.Http.Json;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureOpenTelemetry();
builder.ConfigureDiscordServices();
builder.ConfigureFileStorage();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


var rabbitMqConfig = new RabbitMQConfig();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMqConfig);
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
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


var mongoClient = new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString"));
builder.Services.AddSingleton<IMongoClient>(_ => mongoClient);
builder.Services.AddSingleton<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));

builder.Services.AddMongoDbCollection<BotCommand>();
builder.Services.AddMongoDbCollection<DiscordServer>();

builder.Services.AddServices();
        
builder.Services.AddScoped<DbContext>(c =>
    new DbContext(c.GetRequiredService<MongoDbContext>(), c.GetRequiredService<IMongoCollection<BotCommand>>(), c.GetRequiredService<IMongoCollection<DiscordServer>>()));

builder.Services.AddGrpcClient<XkcdService.XkcdServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5001");
});

builder.Services
    .AddGraphQL()
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMongoDbSorting()
    .AddMongoDbFiltering()
    .AddMongoDbProjections()
    .AddMongoDbPagingProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var discordClient = app.Services.GetRequiredService<DiscordRestClient>();
await discordClient.LoginAsync(TokenType.Bot, builder.Configuration.GetValue<string>("DiscordSettings:BotToken"));
await discordClient.RegisterCommands(builder);
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});
app.MapGraphQL();
app.MapGroup("/")
    .MapDiscordInteractionApi();
app.Run();
