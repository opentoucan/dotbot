using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Dotbot.Api.Dto;

public partial class VehicleMotTest
{
    [JsonConstructor]
    private VehicleMotTest(MotTestResult result, DateTime? completedDate, DateTime? expiryDate,
        int? odometerReadingInMiles,
        OdometerResult odometerReadResult, string? testNumber, List<Defect> defects)
    {
        Result = result;
        CompletedDate = completedDate;
        ExpiryDate = expiryDate;
        OdometerReadingInMiles = odometerReadingInMiles;
        OdometerReadResult = odometerReadResult;
        TestNumber = testNumber;
        Defects = defects;
    }

    public VehicleMotTest(string? result, DateTime? completedDate, DateTime? expiryDate, string? odometerValue,
        string? odometerUnit,
        string? odometerResult, string? motTestNumber)
    {
        Result = result switch
        {
            "PASSED" => MotTestResult.PASSED,
            "FAILED" => MotTestResult.FAILED,
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

    public MotTestResult Result { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public int? OdometerReadingInMiles { get; private set; }
    public OdometerResult OdometerReadResult { get; private set; }
    public string? TestNumber { get; private set; }
    public List<Defect> Defects { get; } = [];

    public void AddDefect(string? defectType, string? defectText, bool? isDangerous)
    {
        Defects.Add(new Defect(defectType, defectText, isDangerous));
    }

    public partial class Defect
    {
        public enum Type
        {
            [Display(Name = "DANGEROUS")] DANGEROUS = 0,
            [Display(Name = "MAJOR")] MAJOR = 1,
            [Display(Name = "MINOR")] MINOR = 2,
            [Display(Name = "FAIL")] FAIL = 3,
            [Display(Name = "ADVISORY")] ADVISORY = 4,

            [Display(Name = "NON SPECIFIC")] NONSPECIFIC = 5,

            [Display(Name = "SYSTEM GENERATED")] SYSTEMGENERATED = 6,

            [Display(Name = "USER ENTERED")] USERENTERED = 7,

            [Display(Name = "PASS AFTER RECTIFICATION")]
            PRS = 8
        }

        [JsonConstructor]
        private Defect(Type? defectType, string? defectMessage, bool isDangerous)
        {
            DefectType = defectType;
            DefectMessage = defectMessage;
            IsDangerous = isDangerous;
        }

        public Defect(string? type, string? text, bool? dangerous)
        {
            if (!Enum.TryParse(MyRegex().Replace(type!, "").ToUpper(), out Type defectType))
                throw new ArgumentException($"Invalid MOT Defect type: {type}");
            DefectType = defectType;
            DefectMessage = text;
            IsDangerous = defectType == Type.DANGEROUS || dangerous.GetValueOrDefault();
        }

        public Type? DefectType { get; set; }

        public string? DefectMessage { get; set; }
        public bool IsDangerous { get; set; }

        [GeneratedRegex(@"\s+")]
        private static partial Regex MyRegex();
    }
}

public enum MotTestResult
{
    PASSED,
    FAILED
}

public enum OdometerResult
{
    READ = 0,
    UNREADABLE = 1,
    NO_ODOMETER = 2
}