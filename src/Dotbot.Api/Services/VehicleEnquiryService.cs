using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ardalis.Result;
using Dotbot.Api.Dto;
using Dotbot.Api.Dto.VesApi;
using Dotbot.Api.Settings;
using Microsoft.Extensions.Options;

namespace Dotbot.Api.Services;

public interface IVehicleEnquiryService
{
    Task<Result<VesApiResponse>> GetVehicleRegistrationInformation(string registrationPlate, CancellationToken cancellationToken = default);
}

public class VehicleEnquiryEnquiryService(HttpClient httpClient, ILogger<VehicleEnquiryEnquiryService> logger) : IVehicleEnquiryService
{
    public async Task<Result<VesApiResponse>> GetVehicleRegistrationInformation(string registrationPlate, CancellationToken cancellationToken = default)
    {
        var vesApiEndpoint = "/vehicle-enquiry/v1/vehicles";

            var httpResponse = await httpClient.PostAsJsonAsync(vesApiEndpoint, new VesApiRequest(registrationPlate), SReadOptions, cancellationToken);
            var vesApiResponse = await JsonSerializer.DeserializeAsync<VesApiResponse>(await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken: cancellationToken);
            if(httpResponse.IsSuccessStatusCode)
                return Result<VesApiResponse>.Success(vesApiResponse!);
            
            var jsonNode = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync(cancellationToken));
            var errorMessage = jsonNode?["message"]?.GetValue<string?>();
            
            logger.LogInformation("Failed to retrieve vehicle registration details");
            
            if(vesApiResponse?.Errors.Count > 0)
                return Result<VesApiResponse>.Error($"{vesApiResponse.Errors.FirstOrDefault()?.Code} - {vesApiResponse.Errors.FirstOrDefault()?.Detail}");
            return Result<VesApiResponse>.Error(errorMessage);
        
    }
    
    private static readonly JsonSerializerOptions SReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
}
