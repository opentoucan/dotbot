using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotbot.Api.Dto;

namespace Dotbot.Api.Services;

public interface IMoturService
{
    Task<ServiceResult<string>> GetRegistrationPlateByVehicleAdvert(string advertUrl,
        CancellationToken cancellationToken = default);
}

public class MoturService(HttpClient httpClient, ILogger<MoturService> logger) : IMoturService
{
    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task<ServiceResult<string>> GetRegistrationPlateByVehicleAdvert(string advertUrl,
        CancellationToken cancellationToken)
    {
        var moturEndpoint = "/link";
        try
        {
            var httpResponse = await httpClient.PostAsync(moturEndpoint,
                new StringContent(JsonSerializer.Serialize(new MoturLinkRequest(advertUrl), SReadOptions),
                    Encoding.UTF8, "application/json"), cancellationToken);
            var registrationPlate = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(registrationPlate))
                return ServiceResult<string>.Success(registrationPlate);
            return ServiceResult<string>.Error("Failed identify the registration plate in the advert");
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Failed to fetch registration plate from endpoint: {moturEndpoint}/{advertUrl} with error: {exception}",
                moturEndpoint, advertUrl, ex);
            return ServiceResult<string>.Error("Could not recognise registration plate from the advert");
        }
    }
}