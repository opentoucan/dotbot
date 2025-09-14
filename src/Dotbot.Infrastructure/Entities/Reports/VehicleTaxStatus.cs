using System.Text.Json.Serialization;

namespace Dotbot.Infrastructure.Entities.Reports;

public class VehicleTaxStatus
{
    public VehicleTaxStatus(string taxStatus, DateTimeOffset? taxDueDate, DateTimeOffset? vehicleRegistrationDate)
    {
        DvlaTaxStatusText = taxStatus;
        TaxDueDate = taxDueDate;
        var vehicleAge = vehicleRegistrationDate - DateTimeOffset.UtcNow;
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
    public VehicleTaxStatus(string? dvlaTaxStatusText, bool isValid, bool isExempt, DateTimeOffset? taxDueDate)
    {
        DvlaTaxStatusText = dvlaTaxStatusText;
        IsValid = isValid;
        IsExempt = isExempt;
        TaxDueDate = taxDueDate;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public string? DvlaTaxStatusText { get; private set; }
    public bool IsValid { get; private set; }
    public bool IsExempt { get; private set; }
    public DateTimeOffset? TaxDueDate { get; private set; }
}