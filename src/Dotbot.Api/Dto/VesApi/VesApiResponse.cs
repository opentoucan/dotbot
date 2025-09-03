namespace Dotbot.Api.Dto.VesApi;

public class Errors
{
    public string? Status { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
}

public class VesApiResponse
{
    public string? RegistrationNumber { get; set; }
    public string? TaxStatus { get; set; }
    public DateTime? TaxDueDate { get; set; }
    public DateTime? ArtExpiryDate { get; set; }
    public string? MotStatus { get; set; }
    public DateTime? MotExpiryDate { get; set; }
    public string? Make { get; set; }
    public string ? MonthOfFirstDvlaRegistration { get; set; }
    public int? YearOfManufacture { get; set; }
    public int? EngineCapacity { get; set; }
    public int? Co2Emissions { get; set; }
    public string? FuelType { get; set; }
    public string? Colour { get; set; }
    public string? TypeApproval { get; set; }
    public string? Wheelplan { get; set; }
    public int? RevenueWeight { get; set; }
    public DateTime? DateOfLastV5cIssued { get; set; }
    public string? EuroStatus { get; set; }
    public bool? AutomatedVehicle { get; set; }
    public List<Errors> Errors { get; set; } = [];
}

