using System.Reflection;
using Dotbot.Database;
using Dotbot.Database.Entities;
using Dotbot.Database.Extensions;
using Dotbot.Database.Settings;
using Dotbot.Ioc;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Dotbot;

internal static class Program
{
    
    public static async Task Main(string[] args)
    {
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


        //builder.Services.AddSingleton<IMongoClient>(mongoClient);
        var db = mongoClient.GetDatabase(section["DatabaseName"]);
        builder.Services.AddSingleton(db);
        builder.Services.AddSingleton<IGridFSBucket>(new GridFSBucket(db));
        builder.Services.AddMongoDbCollection<ChatServer, ChatServerClassMapExtension>("springGuild", new ChatServerClassMapExtension());
        builder.Services.AddMongoDbCollection<BotCommand, DiscordCommandClassMapExtension>("DiscordCommands", new DiscordCommandClassMapExtension());
        builder.Services.AddMongoDbCollection<PersistentSetting, PersistentSettingClassMapExtension>("PersistentSettings", new PersistentSettingClassMapExtension());
        
        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));


        builder.Services.AddServices();
        
        builder.Services.AddSingleton<DbContext>(c =>
            new DbContext(c.GetRequiredService<IMongoCollection<BotCommand>>(),
                c.GetRequiredService<IMongoCollection<ChatServer>>(), 
                c.GetRequiredService<IMongoCollection<PersistentSetting>>()));

        var app = builder.Build();
        
        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}

        //app.UseHttpsRedirection();

        //app.UseAuthorization();

        //app.MapControllers();

   
    }
    
}