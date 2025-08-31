using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result;
using Dotbot.Api.Dto;

namespace Dotbot.Api.Services;

public interface IMotService
{
    Task<Result<MoturResponse>> GetMotByRegistrationPlate(string registrationPlate, CancellationToken cancellationToken = default);
    Task<Result<MoturResponse>> GetMotByVehicleAdvert(string url, CancellationToken cancellationToken = default);
}

public class MotService : IMotService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<XkcdService> _logger;

    public MotService(HttpClient httpClient, ILogger<XkcdService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<MoturResponse>> GetMotByRegistrationPlate(string registrationPlate, CancellationToken cancellationToken = default)
    {
        var moturEndpoint = $"/reg/{registrationPlate}";
        try
        {
            var httpResponse = await _httpClient.GetAsync(moturEndpoint, cancellationToken);
            _logger.LogInformation("MOT Endpoint response {reason phrase}", httpResponse.ReasonPhrase);
            if (httpResponse.IsSuccessStatusCode)
            {
                var motHistory = await JsonSerializer.DeserializeAsync<MoturResponse>(
                    await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken);
                if (motHistory is not null)
                    return Result<MoturResponse>.Success(motHistory);
                return Result.Error($"No MOT history found for {registrationPlate}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch MOT history from endpoint: {moturEndpoint} with error: {exception}", moturEndpoint, ex);
        }

        return Result.Error("Failed to retrieve MOT History");
    }

    public async Task<Result<MoturResponse>> GetMotByVehicleAdvert(string advertUrl, CancellationToken cancellationToken = default)
    {
        var moturEndpoint = "/link";
        try
        {
            var httpResponse = await _httpClient.PostAsync(moturEndpoint, new StringContent(JsonSerializer.Serialize(new MoturLinkRequest(advertUrl), options: SReadOptions), Encoding.UTF8, "application/json"), cancellationToken);
            _logger.LogInformation("MOT Endpoint response {reason phrase}", httpResponse.ReasonPhrase);
            if (httpResponse.IsSuccessStatusCode)
            {
                var motHistory = await JsonSerializer.DeserializeAsync<MoturResponse>(
                    await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken);
                if (motHistory is not null)
                    return Result<MoturResponse>.Success(motHistory);
            }
            return Result.Error(httpResponse.ReasonPhrase);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch MOT history from endpoint: {moturEndpoint}/{advertUrl} with error: {exception}", moturEndpoint, advertUrl, ex);
        }

        return Result.Error("Failed to retrieve MOT History");
    }

    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
}