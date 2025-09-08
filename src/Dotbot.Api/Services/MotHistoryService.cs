using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotbot.Api.Dto.MotApi;
using Dotbot.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using ServiceDefaults;

namespace Dotbot.Api.Services;

public interface IMotHistoryService
{
    Task<ServiceResult<MotApiResponse>> GetVehicleMotHistory(string registrationPlate,
        CancellationToken cancellationToken = default);
}

public interface IMotHistoryAuthenticationProvider
{
    Task<string> GetBearerToken(CancellationToken cancellationToken = default);
}

public class MotHistoryAuthenticationProvider(IOptions<MotHistorySettings> motHistorySettings)
    : IMotHistoryAuthenticationProvider
{
    public async Task<string> GetBearerToken(CancellationToken cancellationToken = default)
    {
        var application = ConfidentialClientApplicationBuilder
            .Create(motHistorySettings.Value.ClientId)
            .WithTenantId(motHistorySettings.Value.TenantId)
            .WithClientSecret(motHistorySettings.Value.ClientSecret)
            .Build();

        var token = await application.AcquireTokenForClient(["https://tapi.dvsa.gov.uk/.default"])
            .ExecuteAsync(cancellationToken);
        return token.AccessToken;
    }
}

public class MotHistoryService(
    HttpClient httpClient,
    ILogger<MotHistoryService> logger,
    IMotHistoryAuthenticationProvider authenticationProvider) : IMotHistoryService
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
        var motHistoryEndpoint = $"/v1/trade/vehicles/registration/{registrationPlate}";


        var accessToken = await authenticationProvider.GetBearerToken(cancellationToken);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var httpResponse = await httpClient.GetAsync(motHistoryEndpoint, cancellationToken);

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

            return ServiceResult<MotApiResponse>.Error(
                $"MOT information not found for the vehicle: {registrationPlate}");
        }

        logger.LogWarning(
            "Failed to fetch MOT history from endpoint: {baseAddress}/{motHistoryEndpoint}/{registrationPlate} with response: {response}",
            httpClient.BaseAddress?.Host, motHistoryEndpoint, registrationPlate, httpResponse.StatusCode);

        Instrumentation.MotApiErrorCounter.Add(1,
            new KeyValuePair<string, object?>("mot_api_error", (int)httpResponse.StatusCode));

        return ServiceResult<MotApiResponse>.Error(
            $"Something went wrong retrieving the MOT history for the vehicle: {registrationPlate}");
    }
}