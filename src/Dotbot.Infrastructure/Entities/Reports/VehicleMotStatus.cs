using System.Text.Json.Serialization;

namespace Dotbot.Infrastructure.Entities.Reports;

public class VehicleMotStatus
{
    public VehicleMotStatus(string? dvlaMotStatus, DateTimeOffset? firstMotDueDate, DateTimeOffset? latestMotExpiryDate,
        DateTimeOffset? vehicleRegistrationDate)
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
    public VehicleMotStatus(bool isValid, DateTimeOffset? validUntil, bool isExempt)
    {
        IsValid = isValid;
        ValidUntil = validUntil;
        IsExempt = isExempt;
    }

    public bool IsValid { get; private set; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public DateTimeOffset? ValidUntil { get; private set; }
    public bool IsExempt { get; private set; }
}