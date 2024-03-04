using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Xkcd.Job.FunctionalTests.Util;

public sealed class XkcdJobFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IContainer MongoDb { get; }

    public XkcdJobFixture()
    {
        MongoDb = new ContainerBuilder()
            .WithName("xkcd-job-mongo-functional-tests")
            .WithImage("mongo:7.0")
            .WithCommand("mongod", "--replSet", "rs0")
            .WithPortBinding("27017", true)
            .WithCleanUp(true)
            .Build();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.Configure(services =>
        {
        });

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "MongoDbSettings:ConnectionString", $"mongodb://localhost:{MongoDb.GetMappedPublicPort("27017")}" },
                { "MongoDbSettings:DatabaseName", "functional-tests" }
            }!);
        });
        builder.ConfigureTestServices(services =>
        {
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());
            });
        });
    }
    
    public async Task InitializeAsync()
    {
        await MongoDb.StartAsync();
        await MongoDb.ExecAsync(new List<string>
        {
            "mongosh", "--eval", "rs.initiate()"
        });
    }

    public new async Task DisposeAsync()
    {
        await MongoDb.StopAsync();
    }
}