using System.Reflection;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Xkcd.Job.FunctionalTests.Util;

public class CustomWebApplicationFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("postgres")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithImage("postgres:16")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.Configure(services => { });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<TDbContext>();
            services.AddDbContext<TDbContext>(options => { options.UseNpgsql(_container.GetConnectionString()); });
            services.EnsureDbCreated<TDbContext>();
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());
            });
        });
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync().AsTask();
}