using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotbot.Api.Dto.VesApi;

namespace Dotbot.Api.Services;

public interface IVehicleEnquiryService
{
    Task<ServiceResult<VesApiResponse>> GetVehicleRegistrationInformation(string registrationPlate,
        CancellationToken cancellationToken = default);
}

public class VehicleEnquiryEnquiryService(HttpClient httpClient, ILogger<VehicleEnquiryEnquiryService> logger)
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

        try
        {
            var httpResponse = await httpClient.PostAsJsonAsync(vesApiEndpoint, new VesApiRequest(registrationPlate),
                SReadOptions, cancellationToken);
            var vesApiResponse = await JsonSerializer.DeserializeAsync<VesApiResponse>(
                await httpResponse.Content.ReadAsStreamAsync(cancellationToken), SReadOptions, cancellationToken);
            if (httpResponse.IsSuccessStatusCode)
                return ServiceResult<VesApiResponse>.Success(vesApiResponse!);

            if (httpResponse.StatusCode == HttpStatusCode.NotFound ||
                httpResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                logger.LogInformation("Failed to retrieve vehicle registration details {registrationPlate}",
                    registrationPlate);
                return ServiceResult<VesApiResponse>.Success();
            }

            return ServiceResult<VesApiResponse>.Error(httpResponse.StatusCode,
                $"Something went wrong retrieving the vehicle registration details for {registrationPlate}");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex,
                "The operation was cancelled while retrieving a response from the Vehicle Enquiry Service API");

            return ServiceResult<VesApiResponse>.Error(ex,
                $"Something went wrong retrieving the vehicle registration details for {registrationPlate}");
        }
    }
}