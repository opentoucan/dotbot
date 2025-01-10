using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Bogus;
using Dotbot.Gateway.Application.Queries;
using Dotbot.Gateway.FunctionalTests.Setup;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCord;
using NetCord.JsonModels;
using Npgsql;
using Org.BouncyCastle.Crypto.Signers;
using Xunit;

namespace Dotbot.Gateway.FunctionalTests;

public class DotbotGatewayTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var dbConnection = new NpgsqlConnection(factory.PostgresConnectionString);
        await dbConnection.OpenAsync();
        var context = new DotbotContext(new DbContextOptionsBuilder<DotbotContext>().UseNpgsql(dbConnection).Options);
        FakeData.PopulateTestData(context);
        context.Guilds.Add(new Guild("123456789", "test"));
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await factory.ResetDatabaseAsync();

    [Fact]
    public async Task NoSignaturesInInteractionRequestGivesUnauthorised()
    {
        var result = await factory.HttpClient.PostAsync("/interactions", new StringContent(JsonSerializer.Serialize(
            new JsonMessageInteraction
            {
                Id = new Faker().Random.ULong(),
                Name = new Faker().Random.String(),
                Type = InteractionType.ApplicationCommand,
                User = new Faker<JsonUser>().Generate()
            }), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task PingHealthCheckInteractionRequestWorks()
    {
        var content = JsonSerializer.Serialize(
            new JsonInteraction
            {
                Id = new Faker().Random.ULong(),
                ApplicationId = new Faker().Random.ULong(),
                Token = new Faker().Random.String(),
                Type = InteractionType.Ping,
                User = new Faker<JsonUser>().Generate(),
                Entitlements = new Faker<JsonEntitlement>().Generate(10).ToArray()
            });
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var message = Encoding.UTF8.GetBytes(timestamp + content);

        var signer = new Ed25519Signer();
        signer.Init(true, factory.PrivateKey);
        signer.BlockUpdate(message, 0, message.Length);

        byte[] signature = signer.GenerateSignature();
        var strSignature = Convert.ToHexString(signature);
        factory.HttpClient.DefaultRequestHeaders.Add("X-Signature-Ed25519", strSignature);
        factory.HttpClient.DefaultRequestHeaders.Add("X-Signature-Timestamp", timestamp.ToString());

        var result = await factory.HttpClient.PostAsync("/interactions",
            new StringContent(content, Encoding.UTF8, "application/json"));

        var interactionResponse = await result.Content.ReadFromJsonAsync<JsonInteraction>();

        using (new AssertionScope())
        {
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            interactionResponse.Should().NotBeNull();
            interactionResponse!.Type.Should().Be(InteractionType.Ping);
        }
    }
}