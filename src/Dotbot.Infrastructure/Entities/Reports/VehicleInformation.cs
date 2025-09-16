using System.Text.Json.Serialization;

namespace Dotbot.Infrastructure.Entities.Reports;

public class VehicleInformation : Entity
{
    public VehicleInformation(string registration,
        bool potentiallyScrapped,
        string? make, string? model, string? colour,
        string? fuelType,
        string? dvlaMotStatus, DateTimeOffset? firstMotDueDate, DateTimeOffset? latestMotExpiryDate,
        string? taxStatus,
        DateTimeOffset? taxDueDate,
        DateTimeOffset? vehicleRegistrationDate, string? engineCapacity, int? weightInKg, int? co2InGramPerKilometer,
        DateTimeOffset? lastIssuedV5CDate)
    {
        Registration = registration;
        PotentiallyScrapped = potentiallyScrapped;
        Make = make;
        Model = model;
        Colour = colour;
        if (fuelType != null && fuelType.Contains("electric", StringComparison.InvariantCultureIgnoreCase))
            FuelType = FuelType.Electric;
        else if (fuelType != null && fuelType.Contains("petrol", StringComparison.InvariantCultureIgnoreCase))
            FuelType = FuelType.Petrol;
        else if (fuelType != null && fuelType.Contains("diesel", StringComparison.InvariantCultureIgnoreCase))
            FuelType = FuelType.Diesel;
        else
            FuelType = FuelType.Unknown;
        MotStatus = new VehicleMotStatus(dvlaMotStatus, firstMotDueDate, latestMotExpiryDate, vehicleRegistrationDate);
        TaxStatus = new VehicleTaxStatus(taxStatus ?? "Unknown", taxDueDate, vehicleRegistrationDate);
        WeightInKg = weightInKg;
        RegistrationDate = vehicleRegistrationDate;
        EngineCapacityLitres = !string.IsNullOrWhiteSpace(engineCapacity)
            ? decimal.Round(decimal.Parse(engineCapacity) / 1000, 1)
            : null;
        Co2InGramPerKilometer = co2InGramPerKilometer;
        LastIssuedV5CDate = lastIssuedV5CDate;
    }

    public VehicleInformation(string registration, bool potentiallyScrapped, string? make, string? model,
        string? colour, FuelType fuelType, DateTimeOffset? registrationDate,
        decimal? engineCapacityLitres, int? weightInKg, int? co2InGramPerKilometer, DateTimeOffset? lastIssuedV5CDate)
    {
        Registration = registration;
        PotentiallyScrapped = potentiallyScrapped;
        Make = make;
        Model = model;
        Colour = colour;
        FuelType = fuelType;
        RegistrationDate = registrationDate;
        EngineCapacityLitres = engineCapacityLitres;
        WeightInKg = weightInKg;
        Co2InGramPerKilometer = co2InGramPerKilometer;
        LastIssuedV5CDate = lastIssuedV5CDate;
    }

    [JsonConstructor]
    private VehicleInformation(string registration, bool potentiallyScrapped, string? make, string? model,
        string? colour, FuelType fuelType, VehicleMotStatus motStatus, VehicleTaxStatus taxStatus,
        DateTimeOffset? registrationDate,
        decimal? engineCapacityLitres, int? weightInKg, int? co2InGramPerKilometer, DateTimeOffset? lastIssuedV5CDate,
        List<VehicleMotTest> vehicleMotTests)
    {
        Registration = registration;
        PotentiallyScrapped = potentiallyScrapped;
        Make = make;
        Model = model;
        Colour = colour;
        FuelType = fuelType;
        MotStatus = motStatus;
        TaxStatus = taxStatus;
        RegistrationDate = registrationDate;
        EngineCapacityLitres = engineCapacityLitres;
        WeightInKg = weightInKg;
        Co2InGramPerKilometer = co2InGramPerKilometer;
        LastIssuedV5CDate = lastIssuedV5CDate;
        VehicleMotTests = vehicleMotTests;
    }

    public string Registration { get; private set; }
    public bool PotentiallyScrapped { get; private set; }
    public string? Make { get; private set; }
    public string? Model { get; private set; }
    public string? Colour { get; private set; }
    public FuelType FuelType { get; private set; }
    public VehicleMotStatus MotStatus { get; private set; } = null!;
    public VehicleTaxStatus TaxStatus { get; private set; } = null!;
    public DateTimeOffset? RegistrationDate { get; private set; }
    public decimal? EngineCapacityLitres { get; private set; }
    public int? WeightInKg { get; private set; }
    public int? Co2InGramPerKilometer { get; private set; }
    public DateTimeOffset? LastIssuedV5CDate { get; private set; }
    public List<VehicleMotTest> VehicleMotTests { get; } = [];

    public void AddMotTest(string? result, DateTimeOffset? completedDate, DateTimeOffset? expiryDate,
        string? odometerValue,
        string? odometerUnit,
        string? odometerResult, string? motTestNumber,
        List<(string? defectType, VehicleMotInspectionDefectDefinition motInspectionDefectDefinition
                , bool? isDangerous)>
            defects)
    {
        var motTest = new VehicleMotTest(result, completedDate, expiryDate, odometerValue, odometerUnit, odometerResult,
            motTestNumber);
        foreach (var defect in defects)
            motTest.AddDefect(defect.defectType, defect.motInspectionDefectDefinition, defect.isDangerous);
        VehicleMotTests.Add(motTest);
    }
}

public enum FuelType
{
    Unknown = 0,
    Petrol = 1,
    Diesel = 2,
    Electric = 3
}