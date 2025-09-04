
using System.Net;
using System.Text.Json;
using Dotbot.Api.Dto.VesApi;
using Dotbot.Api.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using TUnit.Core.Logging;

namespace Dotbot.Api.UnitTests.Services;

public class VehicleEnquiryServiceTests
{

    private HttpClient _httpClient = null!;
    private readonly MockHttpMessageHandler _handler = new();

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

    [Test]
    public async Task GetMotByRegistrationPlate_BadRequest_ReturnsErrorWithReason()
    {
        var reg = "AAA1 AAA";
        var sut = new VehicleEnquiryEnquiryService(_httpClient, Substitute.For<Microsoft.Extensions.Logging.ILogger<VehicleEnquiryEnquiryService>>());

        _handler
          .Expect(HttpMethod.Post, $"{_httpClient.BaseAddress}vehicle-enquiry/v1/vehicles")
          .Respond(HttpStatusCode.BadRequest, [], "application/json", new MemoryStream(await new StringContent(JsonSerializer.Serialize(new VesApiResponse())).ReadAsByteArrayAsync()));

        var response = await sut.GetVehicleRegistrationInformation(reg);

        await Assert.That(response.IsSuccess).IsFalse();
    }

    [Arguments(HttpStatusCode.InternalServerError)]
    [Test]
    public async Task GetMotByRegistrationPlate_UnmappedErrors_ReturnsGenericErrorResponse(HttpStatusCode statusCode)
    {
        var reg = "AAA1 AAA";
        var sut = new VehicleEnquiryEnquiryService(_httpClient, Substitute.For<Microsoft.Extensions.Logging.ILogger<VehicleEnquiryEnquiryService>>());

        _handler
            .Expect(HttpMethod.Post, $"{_httpClient.BaseAddress}vehicle-enquiry/v1/vehicles")
            .Respond(HttpStatusCode.InternalServerError, [], "application/json", new MemoryStream(await new StringContent(JsonSerializer.Serialize(new VesApiResponse())).ReadAsByteArrayAsync()));

        var response = await sut.GetVehicleRegistrationInformation(reg);

        await Assert.That(response.IsSuccess).IsFalse();
    }

}