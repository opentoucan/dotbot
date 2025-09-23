using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities.Reports;
using Dotbot.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Dotbot.Api.Services;

public interface IVehicleInformationService
{
    Task<ServiceResult<VehicleInformation>> GetVehicleInformation(string registrationPlate,
        CancellationToken cancellationToken);

    Task SaveVehicleInformation(string registrationPlate,
        VehicleInformation vehicleInformation,
        string callingUserId,
        string callingGuildId,
        CancellationToken cancellationToken = default);
}

public class VehicleInformationService(
    IVehicleEnquiryService vehicleEnquiryService,
    IMotHistoryService motHistoryService,
    IRedisDatabase redisDatabase,
    ILogger<VehicleInformationService> logger,
    IMotInspectionDefectDefinitionRepository motInspectionDefectDefinitionRepository,
    DotbotContext dbContext) : IVehicleInformationService
{
    public async Task<ServiceResult<VehicleInformation>> GetVehicleInformation(string registrationPlate,
        CancellationToken cancellationToken)
    {
        VehicleInformation? vehicleInformation = null;
        try
        {
            vehicleInformation = await redisDatabase.GetAsync<VehicleInformation?>(registrationPlate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred when fetching vehicle information {registrationPlate} from cache",
                registrationPlate);
        }

        if (vehicleInformation is not null)
        {
            logger.LogInformation("Got a cache hit for {registrationPlate}, skipping API calls", registrationPlate);
        }
        else if (vehicleInformation is null)
        {
            logger.LogInformation("No cache hit for {registrationPlate}", registrationPlate);
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
                        (defect.Type,
                            motInspectionDefectDefinitionRepository.GetByDefectDescription(defect.Text!),
                            defect.Dangerous)).ToList());

            try
            {
                await redisDatabase.AddAsync(registrationPlate, vehicleInformation, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred when writing {registrationPlate} to the cache",
                    registrationPlate);
            }
        }

        return ServiceResult<VehicleInformation>.Success(vehicleInformation);
    }

    public async Task SaveVehicleInformation(string registrationPlate,
        VehicleInformation vehicleInformation,
        string callingUserId,
        string callingGuildId,
        CancellationToken cancellationToken)
    {
        var existingVehicleInformation =
            await dbContext.VehicleInformation.FirstOrDefaultAsync(x => x.Registration == registrationPlate,
                cancellationToken);
        if (existingVehicleInformation is null)
        {
            await dbContext.VehicleInformation.AddAsync(vehicleInformation, cancellationToken);
        }
        else
        {
            vehicleInformation.Id = existingVehicleInformation.Id;
            dbContext.Entry(existingVehicleInformation).CurrentValues.SetValues(vehicleInformation);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}