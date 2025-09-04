using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ardalis.Result;
using Dotbot.Api.Dto;
using Dotbot.Api.Dto.MotApi;
using Dotbot.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Dotbot.Api.Services;

public interface IMotHistoryService
{
    Task<Result<MotApiResponse>> GetVehicleMotHistory(string registrationPlate, CancellationToken cancellationToken = default);
}

public class MotHistoryService(HttpClient httpClient, ILogger<MotHistoryService> logger, IOptions<MotHistorySettings> motHistorySettings) : IMotHistoryService
{
    public async Task<Result<MotApiResponse>> GetVehicleMotHistory(string registrationPlate, CancellationToken cancellationToken = default)
    {
        var motEndpoint = $"/v1/trade/vehicles/registration/{registrationPlate}";
        try
        {
            var application = ConfidentialClientApplicationBuilder
                .Create(motHistorySettings.Value.ClientId)
                .WithTenantId(motHistorySettings.Value.TenantId)
                .WithClientSecret(motHistorySettings.Value.ClientSecret)
                .Build();

            var token = await application.AcquireTokenForClient(["https://tapi.dvsa.gov.uk/.default"]).ExecuteAsync(cancellationToken);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var httpResponse = await httpClient.GetAsync(motEndpoint, cancellationToken);
            var motResponse = await JsonSerializer.DeserializeAsync<MotApiResponse>(await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken: cancellationToken);
            if (httpResponse.IsSuccessStatusCode)
                return Result<MotApiResponse>.Success(motResponse!);
            return Result<MotApiResponse>.Error($"{motResponse?.ErrorCode} - {motResponse?.ErrorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to fetch MOT history from endpoint: {moturEndpoint}/{registrationPlate} with error: {exception}", motEndpoint, registrationPlate, ex);
        }

        return Result.Error($"An error occurred when retrieving MOT history for registration: {registrationPlate}");
    }

    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
}