using System.Text.Json.Serialization;

namespace Dotbot.Api.Dto;

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