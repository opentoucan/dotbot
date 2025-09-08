using System.Text.Json.Serialization;

namespace Dotbot.Infrastructure.Entities;

public class VehicleInformation
{
    public VehicleInformation(string registration,
        bool potentiallyScrapped,
        string? make, string? model, string? colour,
        string? fuelType,
        string? dvlaMotStatus, DateTime? firstMotDueDate, DateTime? latestMotExpiryDate,
        string? taxStatus,
        DateTime? taxDueDate,
        DateTime? vehicleRegistrationDate, string? engineCapacity, int? weightInKg, int? co2InGramPerKilometer,
        DateTime? lastIssuedV5CDate)
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
        MotStatus = new MotStatus(dvlaMotStatus, firstMotDueDate, latestMotExpiryDate, vehicleRegistrationDate);
        TaxStatus = new TaxStatus(taxStatus ?? "Unknown", taxDueDate, vehicleRegistrationDate);
        WeightInKg = weightInKg;
        RegistrationDate = vehicleRegistrationDate;
        EngineCapacityLitres = !string.IsNullOrWhiteSpace(engineCapacity)
            ? decimal.Round(decimal.Parse(engineCapacity) / 1000, 1)
            : null;
        Co2InGramPerKilometer = co2InGramPerKilometer;
        LastIssuedV5CDate = lastIssuedV5CDate;
    }

    [JsonConstructor]
    private VehicleInformation(string registration, bool potentiallyScrapped, string? make, string? model,
        string? colour, FuelType fuelType, MotStatus motStatus, TaxStatus taxStatus, DateTime? registrationDate,
        decimal? engineCapacityLitres, int? weightInKg, int? co2InGramPerKilometer, DateTime? lastIssuedV5CDate,
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
    public MotStatus MotStatus { get; private set; }
    public TaxStatus TaxStatus { get; private set; }
    public DateTime? RegistrationDate { get; private set; }
    public decimal? EngineCapacityLitres { get; private set; }
    public int? WeightInKg { get; private set; }
    public int? Co2InGramPerKilometer { get; private set; }
    public DateTime? LastIssuedV5CDate { get; private set; }
    public List<VehicleMotTest> VehicleMotTests { get; } = [];

    public void AddMotTest(string? result, DateTime? completedDate, DateTime? expiryDate, string? odometerValue,
        string? odometerUnit,
        string? odometerResult, string? motTestNumber,
        List<(string? defectType, string? defectText, bool? isDangerous)> defects)
    {
        var motTest = new VehicleMotTest(result, completedDate, expiryDate, odometerValue, odometerUnit, odometerResult,
            motTestNumber);
        foreach (var defect in defects)
            motTest.AddDefect(defect.defectType, defect.defectText, defect.isDangerous);
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

public class MotStatus
{
    public MotStatus(string? dvlaMotStatus, DateTime? firstMotDueDate, DateTime? latestMotExpiryDate,
        DateTime? vehicleRegistrationDate)
    {
        ValidUntil = firstMotDueDate ?? latestMotExpiryDate;
        var vehicleAge = vehicleRegistrationDate - DateTime.UtcNow;
        if ((dvlaMotStatus != null && dvlaMotStatus.Equals("valid", StringComparison.InvariantCultureIgnoreCase)) ||
            ValidUntil > DateTime.Now)
        {
            IsValid = true;
        }
        else if (vehicleAge?.TotalDays / 365.2425 >= 40)
        {
            IsValid = true;
            IsExempt = true;
        }
    }

    [JsonConstructor]
    private MotStatus(bool isValid, DateTime? validUntil, bool isExempt)
    {
        IsValid = isValid;
        ValidUntil = validUntil;
        IsExempt = isExempt;
    }

    public bool IsValid { get; private set; }
    public DateTime? ValidUntil { get; }
    public bool IsExempt { get; private set; }
}

public class TaxStatus
{
    public TaxStatus(string taxStatus, DateTime? taxDueDate, DateTime? vehicleRegistrationDate)
    {
        DvlaTaxStatusText = taxStatus;
        TaxDueDate = taxDueDate;
        var vehicleAge = vehicleRegistrationDate - DateTime.UtcNow;
        if (DvlaTaxStatusText.Equals("taxed", StringComparison.InvariantCultureIgnoreCase))
        {
            IsValid = true;
        }
        else if (vehicleAge?.TotalDays / 365.2425 >= 40)
        {
            IsValid = true;
            IsExempt = true;
        }
    }

    [JsonConstructor]
    private TaxStatus(string? dvlaTaxStatusText, bool isValid, bool isExempt, DateTime? taxDueDate)
    {
        DvlaTaxStatusText = dvlaTaxStatusText;
        IsValid = isValid;
        IsExempt = isExempt;
        TaxDueDate = taxDueDate;
    }

    public string? DvlaTaxStatusText { get; }
    public bool IsValid { get; private set; }
    public bool IsExempt { get; private set; }
    public DateTime? TaxDueDate { get; private set; }
}