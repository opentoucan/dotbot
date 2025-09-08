using Dotbot.Infrastructure.Entities;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Dotbot.Api.Services;

public interface IVehicleInformationService
{
    Task<ServiceResult<VehicleInformation>> GetVehicleInformation(string registrationPlate,
        CancellationToken cancellationToken);
}

public class VehicleInformationService(
    IVehicleEnquiryService vehicleEnquiryService,
    IMotHistoryService motHistoryService,
    IRedisDatabase redisDatabase) : IVehicleInformationService
{
    public async Task<ServiceResult<VehicleInformation>> GetVehicleInformation(string registrationPlate,
        CancellationToken cancellationToken)
    {
        VehicleInformation? vehicleInformation;
        vehicleInformation = await redisDatabase.GetAsync<VehicleInformation>(registrationPlate);

        if (vehicleInformation == null)
        {
            var vehicleRegistrationResult =
                await vehicleEnquiryService.GetVehicleRegistrationInformation(registrationPlate, cancellationToken);
            var motHistoryResult = await motHistoryService.GetVehicleMotHistory(registrationPlate, cancellationToken);

            var registrationPlateResult = vehicleRegistrationResult.Value?.RegistrationNumber ??
                                          motHistoryResult.Value?.Registration;

            if ((!vehicleRegistrationResult.IsSuccess &&
                 !motHistoryResult.IsSuccess) || string.IsNullOrWhiteSpace(registrationPlate))
                return ServiceResult<VehicleInformation>.Error(string.Join("\n",
                    vehicleRegistrationResult.ErrorResult?.ErrorMessage, motHistoryResult.ErrorResult?.ErrorMessage));


            var potentiallyScrapped = !vehicleRegistrationResult.IsSuccess && motHistoryResult.IsSuccess;

            vehicleInformation = new VehicleInformation(
                registrationPlateResult!,
                potentiallyScrapped,
                vehicleRegistrationResult.Value?.Make ?? motHistoryResult.Value?.Make,
                motHistoryResult.Value?.Model,
                vehicleRegistrationResult.Value?.Colour ?? motHistoryResult.Value?.PrimaryColour,
                vehicleRegistrationResult.Value?.FuelType ?? motHistoryResult.Value?.FuelType,
                vehicleRegistrationResult.Value?.MotStatus,
                motHistoryResult.Value?.MotTestDueDate,
                motHistoryResult.Value?.MotTests.Where(x => x.TestResult == "PASSED")
                    .OrderByDescending(x => x.CompletedDate).FirstOrDefault()?.ExpiryDate,
                vehicleRegistrationResult.Value?.TaxStatus,
                vehicleRegistrationResult.Value?.TaxDueDate,
                !string.IsNullOrWhiteSpace(vehicleRegistrationResult.Value?.MonthOfFirstDvlaRegistration)
                    ? DateTime.Parse($"01-{vehicleRegistrationResult.Value?.MonthOfFirstDvlaRegistration}")
                    : motHistoryResult.Value?.RegistrationDate,
                vehicleRegistrationResult.Value?.EngineCapacity?.ToString() ?? motHistoryResult.Value?.EngineSize,
                vehicleRegistrationResult.Value?.RevenueWeight,
                vehicleRegistrationResult.Value?.Co2Emissions,
                vehicleRegistrationResult.Value?.DateOfLastV5cIssued);

            foreach (var motTest in motHistoryResult.Value?.MotTests ?? [])
                vehicleInformation.AddMotTest(
                    motTest.TestResult,
                    motTest.CompletedDate,
                    motTest.ExpiryDate,
                    motTest.OdometerValue,
                    motTest.OdometerUnit,
                    motTest.OdometerResultType,
                    motTest.MotTestNumber,
                    motTest.Defects.Select(defect =>
                        (defect.Type, defect.Text, defect.Dangerous)).ToList());

            await redisDatabase.AddAsync(registrationPlate, vehicleInformation);
        }

        return ServiceResult<VehicleInformation>.Success(vehicleInformation);
    }
}