using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result;

namespace Dotbot.Api.Services;

public interface IMotService
{
    Task<Result<MotHistory>> GetMotByRegistrationPlate(string registrationPlate, CancellationToken cancellationToken = default);
    Task<Result<MotHistory>> GetMotByVehicleAdvert(string url, CancellationToken cancellationToken = default);
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

    public async Task<Result<MotHistory>> GetMotByRegistrationPlate(string registrationPlate, CancellationToken cancellationToken = default)
    {
        var url = $"/reg?reg={registrationPlate}";
        try
        {
            var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
            _logger.LogInformation($"MOT Endpoint response {await httpResponse.Content.ReadAsStringAsync()}");
            if (httpResponse.IsSuccessStatusCode)
            {
                var motHistory = await JsonSerializer.DeserializeAsync<MotHistory>(
                    await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken);
                if (motHistory is not null)
                    return Result<MotHistory>.Success(motHistory);
                return Result.Error($"No MOT history found for {registrationPlate}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch MOT history from endpoint: {url} with error: {exception}", url, ex);
        }

        return Result.Error("Failed to retrieve MOT History");
    }

    public async Task<Result<MotHistory>> GetMotByVehicleAdvert(string advertUrl, CancellationToken cancellationToken = default)
    {
        var url = $"/link?url={advertUrl}";
        try
        {
            var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
            _logger.LogInformation($"MOT Endpoint response {await httpResponse.Content.ReadAsStringAsync()}");
            if (httpResponse.IsSuccessStatusCode)
            {
                var motHistory = await JsonSerializer.DeserializeAsync<MotHistory>(
                    await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken);
                if (motHistory is not null)
                    return Result<MotHistory>.Success(motHistory);
            }
            return Result.Error(httpResponse.ReasonPhrase);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch MOT history from endpoint: {url} with error: {exception}", url, ex);
        }

        return Result.Error("Failed to retrieve MOT History");
    }

    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
}

public enum DefectType
{
    [JsonStringEnumMemberName("DANGEROUS")]
    DANGEROUS = 0,
    [JsonStringEnumMemberName("MAJOR")]
    MAJOR = 1,
    [JsonStringEnumMemberName("MINOR")]
    MINOR = 2,
    [JsonStringEnumMemberName("FAIL")]
    FAIL = 3,
    [JsonStringEnumMemberName("ADVISORY")]
    ADVISORY = 4,
    [JsonStringEnumMemberName("NON SPECIFIC")]
    NONSPECIFIC = 5,
    [JsonStringEnumMemberName("SYSTEM GENERATED")]
    SYSTEMGENERATED = 6,
    [JsonStringEnumMemberName("USER ENTERED")]
    USERENTERED = 7,
    [JsonStringEnumMemberName("PRS")]
    PRS = 8
}

public class Defect
{
    public required bool dangerous { get; set; }
    public required string text { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<DefectType>))]
    public required DefectType type { get; set; }
}

public class MotTest
{
    public object? registrationAtTimeOfTest { get; set; }
    public string? motTestNumber { get; set; }
    public DateTime completedDate { get; set; }
    public string? expiryDate { get; set; }
    public string? odometerValue { get; set; }
    public string? odometerUnit { get; set; }
    public string? odometerResultType { get; set; }
    public string? testResult { get; set; }
    public string? dataSource { get; set; }
    public required List<Defect> defects { get; set; }
}

public class MotHistory
{
    public required string registration { get; set; }
    public required string make { get; set; }
    public required string model { get; set; }
    public string? firstUsedDate { get; set; }
    public required string fuelType { get; set; }
    public required string primaryColour { get; set; }
    public required string registrationDate { get; set; }
    public required string manufactureDate { get; set; }
    public string? engineSize { get; set; }
    public required string hasOutstandingRecall { get; set; }
    public List<MotTest>? motTests { get; set; }
}