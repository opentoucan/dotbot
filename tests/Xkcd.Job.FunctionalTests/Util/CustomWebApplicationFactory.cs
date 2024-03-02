using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Xkcd.Job.FunctionalTests.Util;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{

    // Gives a fixture an opportunity to configure the application before it gets built.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.Configure(services =>
        {
        });
         builder.ConfigureServices(services =>
         {
         });

        builder.ConfigureTestServices(services =>
        {
            // Remove AppDbContext

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoClient));
            if (descriptor != null) services.Remove(descriptor);
            var descriptor2 = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase));
            if (descriptor2 != null) services.Remove(descriptor2);
            // Add DB context pointing to test container
            services.AddSingleton<IMongoClient>(_ => new MongoClient("mongodb://localhost:27017"));
            services.AddSingleton<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase("functional-tests"));
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());
            });
        });
    }

    private readonly IContainer _mongoDbContainer = new ContainerBuilder()
        .WithName("xkcd-job-mongo-functional-tests")
        .WithImage("mongo:7.0")
        .WithCommand("mongod", "--replSet", "rs0")
        .WithPortBinding("27017")
        .WithCleanUp(true)
        .Build();

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
        await _mongoDbContainer.ExecAsync(new List<string>
        {
            "mongosh", "--eval", "rs.initiate()"
        });
    }

    public new async Task DisposeAsync() => await _mongoDbContainer.DisposeAsync();
}