namespace Dotbot.Api.Dto.MotApi;

public class MotApiResponse
{
    public string? Registration { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? FuelType { get; set; }
    public string? PrimaryColour { get; set; }
    public string? EngineSize { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? MotTestDueDate { get; set; }
    public List<MotTest> MotTests { get; set; } = [];
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RequestId { get; set; }

    public class MotTest
    {
        public DateTime? CompletedDate { get; set; }
        public string? TestResult { get; set; }
        public string? OdometerResultType { get; set; }
        public string? DataSource { get; set; }
        public string? RegistrationAtTimeOfTest { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? OdometerValue { get; set; }
        public string? OdometerUnit { get; set; }
        public string? MotTestNumber { get; set; }
        public required List<Defect> Defects { get; set; }

        public class Defect
        {
            public bool? Dangerous { get; set; }
            public string? Text { get; set; }
            public string? Type { get; set; }
        }
    }
}