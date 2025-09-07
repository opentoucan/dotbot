using System.Net;
using System.Text.Json;
using Dotbot.Api.Dto.MotApi;
using Dotbot.Api.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using ServiceDefaults;

namespace Dotbot.Api.UnitTests.Services;

public class MotHistoryServiceTests
{
    private readonly MockHttpMessageHandler _handler = new();

    private HttpClient _httpClient = null!;

    [Before(Test)]
    public Task Before()
    {
        _httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
        return Task.CompletedTask;
    }

    [After(Test)]
    public Task TearDown()
    {
        _httpClient.Dispose();
        return Task.CompletedTask;
    }

    [Arguments(HttpStatusCode.OK)]
    [Test]
    public async Task GetVehicleMotHistory_Ok_ReturnsSuccess(
        HttpStatusCode statusCode)
    {
        var reg = "AAA1 AAA";
        var sut = new MotHistoryService(_httpClient,
            Substitute.For<ILogger<MotHistoryService>>(),
            Substitute.For<Instrumentation>(),
            Substitute.For<IMotHistoryAuthenticationProvider>());

        _handler
            .Expect(HttpMethod.Get, $"{_httpClient.BaseAddress}v1/trade/vehicles/registration/{reg}")
            .Respond(statusCode, [], "application/json",
                new MemoryStream(await new StringContent(JsonSerializer.Serialize(new MotApiResponse()))
                    .ReadAsByteArrayAsync()));

        var response = await sut.GetVehicleMotHistory(reg);

        await Assert.That(response.IsSuccess).IsTrue();
        await Assert.That(response.ErrorResult).IsNull();
    }

    [Arguments(HttpStatusCode.InternalServerError)]
    [Arguments(HttpStatusCode.Forbidden)]
    [Arguments(HttpStatusCode.Unauthorized)]
    [Arguments(HttpStatusCode.RequestTimeout)]
    [Arguments(HttpStatusCode.NotFound)]
    [Test]
    public async Task GetVehicleMotHistory_UnmappedHttpErrors_ReturnsErrorResponse(
        HttpStatusCode statusCode)
    {
        var reg = "AAA1 AAA";
        var sut = new MotHistoryService(_httpClient,
            Substitute.For<ILogger<MotHistoryService>>(),
            Substitute.For<Instrumentation>(),
            Substitute.For<IMotHistoryAuthenticationProvider>());

        _handler
            .Expect(HttpMethod.Get, $"{_httpClient.BaseAddress}v1/trade/vehicles/registration/{reg}")
            .Respond(statusCode, [], "application/json",
                new MemoryStream(await new StringContent(JsonSerializer.Serialize(new MotApiResponse()))
                    .ReadAsByteArrayAsync()));

        var response = await sut.GetVehicleMotHistory(reg);

        await Assert.That(response.IsSuccess).IsFalse();
        await Assert.That(response.Value).IsNull();
        await Assert.That(response.ErrorResult).IsNotNull();
    }
}