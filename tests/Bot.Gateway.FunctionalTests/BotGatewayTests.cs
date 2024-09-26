using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Bogus;
using Bot.Gateway.Application.IntegrationEvents.EventHandlers;
using Bot.Gateway.Application.Queries;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Bot.Gateway.FunctionalTests.Setup;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Repositories;
using Contracts.MessageBus;
using Discord;
using FluentAssertions;
using FluentAssertions.Execution;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Attachment = Bot.Gateway.Dto.Requests.Discord.Attachment;
using InteractionResponseType = Bot.Gateway.Dto.Responses.Discord.InteractionResponseType;

namespace Bot.Gateway.FunctionalTests;

public class BotGatewayTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        FakeData.GenerateData();
        var dbConnection = new NpgsqlConnection(factory.PostgresConnectionString);
        await dbConnection.OpenAsync();
        FakeData.PopulateTestData(new DotbotContext(new DbContextOptionsBuilder<DotbotContext>().UseNpgsql(dbConnection).Options));
    }

    public async Task DisposeAsync() => await factory.ResetDatabaseAsync();
    
    [Fact]
    public async Task InvalidInteractionRequestFails()
    {
        var result = await factory.HttpClient.PostAsync("/api/interactions", new StringContent(JsonSerializer.Serialize(
            new InteractionRequest
            {
                Guild = new Guild
                {
                    Id = Guid.NewGuid().ToString(), 
                    Features = new List<string>()
                }
            }), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task HealthCheckInteractionRequestWorks()
    {
        var requestDataFaker = new Faker<InteractionRequest>();
        var requestData = requestDataFaker
            .RuleFor(req => req.Type, _ => (int)InteractionType.Ping)
            .Generate();
        var result = await factory.HttpClient.PostAsync("/api/interactions", new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json"));

        var interactionResponse = await result.Content.ReadFromJsonAsync<InteractionResponse>();

        using (new AssertionScope())
        {
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            interactionResponse.Should().NotBeNull();
            interactionResponse!.Type.Should().Be(InteractionResponseType.Ping);
        }
    }

    [Fact]
    public async Task SaveInteractionRequestWorks()
    {
        var requestData = new InteractionRequestFaker(new CustomCommandRequestFaker()).Generate();

        var result = await factory.HttpClient.PostAsync("/api/interactions", new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json"));
        var interactionResponse = await result.Content.ReadFromJsonAsync<InteractionResponse>();
        
        
        using var scope = factory.Services.CreateScope();
        var testHarness = factory.Services.GetTestHarness();
        var consumerHarness = testHarness.GetConsumerHarness<DeferredInteractionEventHandler>();
        await testHarness.InactivityTask;
        var queries = scope.ServiceProvider.GetRequiredService<ICustomCommandQueries>();
        var customCommands = await queries.GetCustomCommandsFromServerAsync(requestData.Guild!.Id);
        
        using (new AssertionScope())
        {
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            interactionResponse.Should().NotBeNull();
            interactionResponse!.Type.Should().Be(InteractionResponseType.DeferredInteractionResponse);
            (await consumerHarness.Consumed.Any<DeferredInteractionEvent>()).Should().Be(true);
            customCommands.Should().Contain(x => 
                x.Name == requestData.Data!.Options!.First().SubOptions![0].Value!.ToString() && 
                x.Content == requestData.Data!.Options!.First().SubOptions![1].Value!.ToString() &&
                x.Guild.ExternalId == requestData.Guild!.Id);
        }

    }
}