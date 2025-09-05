using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotbot.Api.Dto.MotApi;
using Dotbot.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Dotbot.Api.Services;

public interface IMotHistoryService
{
    Task<ServiceResult<MotApiResponse>> GetVehicleMotHistory(string registrationPlate,
        CancellationToken cancellationToken = default);
}

public class MotHistoryService(
    HttpClient httpClient,
    ILogger<MotHistoryService> logger,
    IOptions<MotHistorySettings> motHistorySettings) : IMotHistoryService
{
    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task<ServiceResult<MotApiResponse>> GetVehicleMotHistory(string registrationPlate,
        CancellationToken cancellationToken = default)
    {
        var motEndpoint = $"/v1/trade/vehicles/registration/{registrationPlate}";
        try
        {
            var application = ConfidentialClientApplicationBuilder
                .Create(motHistorySettings.Value.ClientId)
                .WithTenantId(motHistorySettings.Value.TenantId)
                .WithClientSecret(motHistorySettings.Value.ClientSecret)
                .Build();

            var token = await application.AcquireTokenForClient(["https://tapi.dvsa.gov.uk/.default"])
                .ExecuteAsync(cancellationToken);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var httpResponse = await httpClient.GetAsync(motEndpoint, cancellationToken);

            if (httpResponse.IsSuccessStatusCode)
            {
                var motResponse = await JsonSerializer.DeserializeAsync<MotApiResponse>(
                    await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken);
                return ServiceResult<MotApiResponse>.Success(motResponse!);
            }

            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation("Failed to retrieve MOT information for the vehicle: {registrationPlate}",
                    registrationPlate);

                return ServiceResult<MotApiResponse>.Error(HttpStatusCode.NotFound,
                    $"MOT information not found for the vehicle: {registrationPlate}");
            }

            logger.LogWarning(
                "Failed to fetch MOT history from endpoint: {moturEndpoint}/{registrationPlate} with response: {response}",
                motEndpoint, registrationPlate, httpResponse.StatusCode);

            return ServiceResult<MotApiResponse>.Error(httpResponse.StatusCode,
                $"Something went wrong retrieving the MOT history for the vehicle: {registrationPlate}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error occurred when retrieving a response from the MOT History API");
            return ServiceResult<MotApiResponse>.Error(ex,
                $"Something went wrong retrieving the MOT history for the vehicle: {registrationPlate}");
        }
    }
}