using System.Net;
using System.Text.Json;
using Dotbot.Api.Dto.VesApi;
using Dotbot.Api.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace Dotbot.Api.UnitTests.Services;

public class VehicleEnquiryServiceTests
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
    public async Task GetVehicleRegistrationInformation_Ok_ReturnsSuccess(
        HttpStatusCode statusCode)
    {
        var reg = "AAA1 AAA";
        var sut = new VehicleEnquiryEnquiryService(_httpClient,
            Substitute.For<ILogger<VehicleEnquiryEnquiryService>>());

        _handler
            .Expect(HttpMethod.Post, $"{_httpClient.BaseAddress}vehicle-enquiry/v1/vehicles")
            .Respond(statusCode, [], "application/json",
                new MemoryStream(await new StringContent(JsonSerializer.Serialize(new VesApiResponse()))
                    .ReadAsByteArrayAsync()));

        var response = await sut.GetVehicleRegistrationInformation(reg);

        await Assert.That(response.IsSuccess).IsTrue();
        await Assert.That(response.ErrorResult).IsNull();
    }

    [Arguments(HttpStatusCode.InternalServerError)]
    [Arguments(HttpStatusCode.Forbidden)]
    [Arguments(HttpStatusCode.Unauthorized)]
    [Arguments(HttpStatusCode.RequestTimeout)]
    [Arguments(HttpStatusCode.BadRequest)]
    [Arguments(HttpStatusCode.NotFound)]
    [Test]
    public async Task GetVehicleRegistrationInformation_HttpErrors_ReturnsErrorResponse(
        HttpStatusCode statusCode)
    {
        var reg = "AAA1 AAA";
        var sut = new VehicleEnquiryEnquiryService(_httpClient,
            Substitute.For<ILogger<VehicleEnquiryEnquiryService>>());

        _handler
            .Expect(HttpMethod.Post, $"{_httpClient.BaseAddress}vehicle-enquiry/v1/vehicles")
            .Respond(statusCode, [], "application/json",
                new MemoryStream(await new StringContent(JsonSerializer.Serialize(new VesApiResponse()))
                    .ReadAsByteArrayAsync()));

        var response = await sut.GetVehicleRegistrationInformation(reg);

        await Assert.That(response.IsSuccess).IsFalse();
        await Assert.That(response.Value).IsNull();
        await Assert.That(response.ErrorResult).IsNotNull();
    }
}