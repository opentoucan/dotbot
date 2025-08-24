using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result;

namespace Dotbot.Api.Services;

public interface IMotService
{
    Task<Result<MoturResponse>> GetMotByRegistrationPlate(string registrationPlate, CancellationToken cancellationToken = default);
    Task<Result<MoturResponse>> GetMotByVehicleAdvert(string url, CancellationToken cancellationToken = default);
}

public class MotService : IMotService
{
    private record LinkRequestBody(string Url);

    private readonly HttpClient _httpClient;
    private readonly ILogger<XkcdService> _logger;

    public MotService(HttpClient httpClient, ILogger<XkcdService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<MoturResponse>> GetMotByRegistrationPlate(string registrationPlate, CancellationToken cancellationToken = default)
    {
        var url = $"/reg/{registrationPlate}";
        try
        {
            var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
            _logger.LogInformation($"MOT Endpoint response {await httpResponse.Content.ReadAsStringAsync()}");
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
            _logger.LogError("Failed to fetch MOT history from endpoint: {url} with error: {exception}", url, ex);
        }

        return Result.Error("Failed to retrieve MOT History");
    }

    public async Task<Result<MoturResponse>> GetMotByVehicleAdvert(string advertUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpResponse = await _httpClient.PostAsync("/link", new StringContent(JsonSerializer.Serialize(new LinkRequestBody(advertUrl), options: SReadOptions), Encoding.UTF8, "application/json"), cancellationToken);
            _logger.LogInformation($"MOT Endpoint response {await httpResponse.Content.ReadAsStringAsync()}");
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
            _logger.LogError("Failed to fetch MOT history from endpoint: {url} with error: {exception}", advertUrl, ex);
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

public class MoturResponse
{
    public RegistrationResponse? RegistrationResponse { get; set; }
    public MotResponse? MotResponse { get; set; }
}

public class ErrorDetails
{
    public string? InternalCode { get; set; }
    public int HttpStatusCode { get; set; }
    public required string Reason { get; set; }
}

public class RegistrationResponse
{
    public class RegistrationDetails
    {
        public required string RegistrationPlate { get; set; }
        public string? TaxStatus { get; set; }
        public DateTime? TaxDueDate { get; set; }
        public string? MotStatus { get; set; }
        public DateTime? MotExpiryDate { get; set; }
        public string? Make { get; set; }
        public int? YearOfManufacture { get; set; }
        public int? EngineCapacityInCc { get; set; }
        public int? Co2EmissionsInGramPerKm { get; set; }
        public string? FuelType { get; set; }
        public string? Colour { get; set; }
        public string? ApprovalCategory { get; set; }
        public string? Wheelplan { get; set; }
        public int? RevenueWeightInKg { get; set; }
        public DateTime? DateOfLastV5cIssued { get; set; }
    }
    public RegistrationDetails? Details { get; set; }
    public ErrorDetails? ErrorDetails { get; set; }
}

public class MotResponse
{
    public class MotDetails
    {
        public class MotTest
        {
            public class Defect
            {
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
                public bool? Dangerous { get; set; }
                public string? Text { get; set; }
                [JsonConverter(typeof(JsonStringEnumConverter<DefectType>))]
                public DefectType? Type { get; set; }
            }
            public DateTime? CompletedDate { get; set; }
            public string? TestResult { get; set; }
            public string? OdometerReadResult { get; set; }
            public string? DataSource { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public string? OdometerValue { get; set; }
            public string? OdometerUnit { get; set; }
            public string? MotTestNumber { get; set; }
            public required List<Defect> Defects { get; set; }
        }
        public string? RegistrationPlate { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? FuelType { get; set; }
        public string? Colour { get; set; }
        public string? EngineSize { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? FirstMotTestDueDate { get; set; }
        public List<MotTest>? MotTests { get; set; }
    }
    public MotDetails? Details { get; set; }
    public ErrorDetails? ErrorDetails { get; set; }
}