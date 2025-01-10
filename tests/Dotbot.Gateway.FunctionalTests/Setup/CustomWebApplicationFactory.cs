using System.Data.Common;
using System.Net;
using System.Reflection;
using Bogus;
using Dotbot.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Rest;
using Npgsql;
using NSubstitute;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Respawn;
using Testcontainers.LocalStack;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Dotbot.Gateway.FunctionalTests.Setup;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("dotbot")
        .WithUsername("dotbot")
        .WithPassword("yourWeak(!)Password")
        .WithImage("postgres:16")
        .WithPortBinding(54320, 5432)
        .WithCleanUp(true)
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithPortBinding(5672, true)
        .Build();

    private readonly LocalStackContainer _localStackContainer = new LocalStackBuilder()
        .WithImage("localstack/localstack:2.0")
        .Build();

    public Ed25519PrivateKeyParameters PrivateKey = null!;
    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    public System.Net.Http.HttpClient HttpClient { get; private set; } = null!;
    public string PostgresConnectionString { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var keyPairGenerator = new Ed25519KeyPairGenerator();
        keyPairGenerator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        var keyPair = keyPairGenerator.GenerateKeyPair();
        PrivateKey = (Ed25519PrivateKeyParameters)keyPair.Private;
        var publicKey = ((Ed25519PublicKeyParameters)keyPair.Public).GetEncoded();
        Environment.SetEnvironmentVariable("Discord__PublicKey", Convert.ToHexStringLower(publicKey));
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "localstack");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "localstack");
        Environment.SetEnvironmentVariable("S3__ServiceURL", $"{_localStackContainer.GetConnectionString()}");
        
        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<DotbotContext>();
            services.AddDbContext<DotbotContext>(options => { options.UseNpgsql(_dbContainer.GetConnectionString()); });
            services.EnsureDbCreated<DotbotContext>();
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_rabbitMqContainer.Hostname,
                        _rabbitMqContainer.GetMappedPublicPort(5672),
                        "/",
                        h =>
                        {
                            h.Username("rabbitmq");
                            h.Password("rabbitmq");
                        });
                    cfg.UseDelayedMessageScheduler();
                    cfg.ConfigureEndpoints(context);
                });
                x.SetTestTimeouts(testInactivityTimeout: TimeSpan.FromSeconds(10), testTimeout: TimeSpan.FromSeconds(10));
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
        await _localStackContainer.StartAsync();

        PostgresConnectionString = _dbContainer.GetConnectionString();
        _dbConnection = new NpgsqlConnection(PostgresConnectionString);
        HttpClient = CreateClient();

        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres
        });
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
        await _dbConnection.DisposeAsync();
        await _localStackContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
}