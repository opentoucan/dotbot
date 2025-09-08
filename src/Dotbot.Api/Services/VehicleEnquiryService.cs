using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotbot.Api.Dto.VesApi;
using ServiceDefaults;

namespace Dotbot.Api.Services;

public interface IVehicleEnquiryService
{
    Task<ServiceResult<VesApiResponse>> GetVehicleRegistrationInformation(string registrationPlate,
        CancellationToken cancellationToken = default);
}

public class VehicleEnquiryEnquiryService(
    HttpClient httpClient,
    ILogger<VehicleEnquiryEnquiryService> logger)
    : IVehicleEnquiryService
{
    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task<ServiceResult<VesApiResponse>> GetVehicleRegistrationInformation(string registrationPlate,
        CancellationToken cancellationToken = default)
    {
        var vesApiEndpoint = "/vehicle-enquiry/v1/vehicles";

        var httpResponse = await httpClient.PostAsJsonAsync(vesApiEndpoint, new VesApiRequest(registrationPlate),
            SReadOptions, cancellationToken);
        var vesApiResponse = await JsonSerializer.DeserializeAsync<VesApiResponse>(
            await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken);
        if (httpResponse.IsSuccessStatusCode)
            return ServiceResult<VesApiResponse>.Success(vesApiResponse!);

        if (httpResponse.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
        {
            logger.LogInformation("No DVLA registration details held for {registrationPlate}",
                registrationPlate);
            return ServiceResult<VesApiResponse>.Error($"No DVLA Data found for {registrationPlate}");
        }

        logger.LogWarning(
            "Failed to fetch vehicle registration data from endpoint: {baseAddress}/{vesApiEndpoint}/{registrationPlate} with response: {response}",
            httpClient.BaseAddress?.Host, vesApiEndpoint, registrationPlate, httpResponse.StatusCode);

        Instrumentation.VesApiErrorCounter.Add(1,
            new KeyValuePair<string, object?>("ves_api_error", (int)httpResponse.StatusCode));

        return ServiceResult<VesApiResponse>.Error(
            $"Something went wrong retrieving the vehicle registration details for {registrationPlate}");
    }
}