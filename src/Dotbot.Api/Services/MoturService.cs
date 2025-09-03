using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result;
using Dotbot.Api.Dto;

namespace Dotbot.Api.Services;

public interface IMoturService
{
    Task<Result<string>> GetRegistrationPlateByVehicleAdvert(string advertUrl, CancellationToken cancellationToken = default);
}

public class MoturService(HttpClient httpClient, ILogger<MoturService> logger) : IMoturService
{
    public async Task<Result<string>> GetRegistrationPlateByVehicleAdvert(string advertUrl, CancellationToken cancellationToken)
    {
        var moturEndpoint = "/link";
        try
        {
            var httpResponse = await httpClient.PostAsync(moturEndpoint, new StringContent(JsonSerializer.Serialize(new MoturLinkRequest(advertUrl), options: SReadOptions), Encoding.UTF8, "application/json"), cancellationToken);
            var registrationPlate = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            if(!string.IsNullOrWhiteSpace(registrationPlate))
                return Result<string>.Success(registrationPlate);
            return Result.Error("Failed identify the registration plate in the advert");
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to fetch registration plate from endpoint: {moturEndpoint}/{advertUrl} with error: {exception}", moturEndpoint, advertUrl, ex);
            return Result.Error("Could not recognise registration plate from the advert");
        }
    }
    
    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

}