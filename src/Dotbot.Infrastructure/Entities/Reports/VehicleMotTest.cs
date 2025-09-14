using System.Globalization;
using System.Text.Json.Serialization;

namespace Dotbot.Infrastructure.Entities.Reports;

public class VehicleMotTest : Entity
{
    [JsonConstructor]
    private VehicleMotTest(TestResult result, DateTimeOffset? completedDate, DateTimeOffset? expiryDate,
        int? odometerReadingInMiles,
        OdometerResult odometerReadResult, string? testNumber, List<VehicleMotTestDefect> defects)
    {
        Result = result;
        CompletedDate = completedDate;
        ExpiryDate = expiryDate;
        OdometerReadingInMiles = odometerReadingInMiles;
        OdometerReadResult = odometerReadResult;
        TestNumber = testNumber;
        Defects = defects;
    }

    public VehicleMotTest(TestResult result, DateTimeOffset? completedDate, DateTimeOffset? expiryDate,
        int? odometerReadingInMiles,
        OdometerResult odometerReadResult, string? testNumber)
    {
        Result = result;
        CompletedDate = completedDate;
        ExpiryDate = expiryDate;
        OdometerReadingInMiles = odometerReadingInMiles;
        OdometerReadResult = odometerReadResult;
        TestNumber = testNumber;
    }

    public VehicleMotTest(string? result, DateTimeOffset? completedDate, DateTimeOffset? expiryDate,
        string? odometerValue,
        string? odometerUnit,
        string? odometerResult, string? motTestNumber)
    {
        Result = result switch
        {
            "PASSED" => TestResult.PASSED,
            "FAILED" => TestResult.FAILED,
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };

        CompletedDate = completedDate;
        ExpiryDate = expiryDate;

        var validOdometerUnits = new List<string> { "MI", "KM" };
        if (!int.TryParse(odometerValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var num) ||
            !validOdometerUnits.Contains(odometerUnit, StringComparer.InvariantCultureIgnoreCase))
            OdometerReadingInMiles = null;
        else if (odometerUnit?.ToUpperInvariant() == "MI")
            OdometerReadingInMiles = num;
        else
            OdometerReadingInMiles = (int)Math.Ceiling(num / 1.609);

        if (!Enum.TryParse(odometerResult, out OdometerResult odometerReadResult))
            throw new ArgumentOutOfRangeException(nameof(odometerResult));
        OdometerReadResult = odometerReadResult;
        TestNumber = motTestNumber;
    }

    public TestResult Result { get; private set; }
    public DateTimeOffset? CompletedDate { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public int? OdometerReadingInMiles { get; private set; }
    public OdometerResult OdometerReadResult { get; private set; }
    public string? TestNumber { get; private set; }
    public List<VehicleMotTestDefect> Defects { get; } = [];

    public void AddDefect(string? defectType, VehicleMotInspectionDefectDefinition motInspectionDefectDefinition,
        bool? isDangerous)
    {
        Defects.Add(new VehicleMotTestDefect(defectType, motInspectionDefectDefinition, isDangerous));
    }
}

public enum TestResult
{
    PASSED = 0,
    FAILED = 1
}

public enum OdometerResult
{
    READ = 0,
    UNREADABLE = 1,
    NO_ODOMETER = 2
}