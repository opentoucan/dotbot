using System.Reflection;
using Dotbot.Extensions;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.GraphQL;
using Dotbot.Ioc;
using Dotbot.Models;
using Dotbot.Settings;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
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

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddSwaggerGen();
        builder.Services.AddSwaggerGen(config =>
        {
            config.SwaggerDoc("v1", new OpenApiInfo { Title = "Dotbot API", Version = "v1" });
        });

        var section = builder.Configuration.GetSection("MongoDbSettings");
        var settings = MongoClientSettings.FromConnectionString(section["ConnectionString"]);
        var mongoClient = new MongoClient(settings);


        //builder.Services.AddSingleton<IMongoClient>(mongoClient);
        var db = mongoClient.GetDatabase(section["DatabaseName"]);
        builder.Services.AddSingleton(db);
        builder.Services.AddMongoDbCollection<BotCommand>();
        
        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
        
        builder.Services.AddServices();
        
        builder.Services.AddSingleton<DbContext>(c =>
            new DbContext(c.GetRequiredService<IMongoCollection<BotCommand>>()));

        
        builder.Services.AddGrpcClient<XkcdService.XkcdServiceClient>(o =>
        {
            o.Address = new Uri("http://localhost:5001");
        });

        builder.Services
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

        //app.UseAuthorization();

        app.MapGraphQL("/graphql");
        app.MapControllers();
        app.Run();

        using (var fileStream = new FileStream("./file.txt", FileMode.Open))
        {
            var bytes = File.ReadAllBytes("./file.txt");
            File.WriteAllBytes("./file2.txt", bytes);
        }

    }
    
}