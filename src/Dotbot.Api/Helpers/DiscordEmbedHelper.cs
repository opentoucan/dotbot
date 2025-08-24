using System.Globalization;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using static Dotbot.Api.Services.MotResponse.MotDetails;
using static Dotbot.Api.Services.MotResponse.MotDetails.MotTest.Defect;

namespace Dotbot.Api.Helpers;

public static class DiscordEmbedHelper
{
    public static Color Green = new(0, 128, 0);
    public static Color Orange = new(255, 140, 0);
    public static Color Red = new(255, 0, 0);
    public static Color Black = new(0, 0, 0);
    public static EmbedProperties BuildVehicleInformationEmbed(MoturResponse moturResponse)
    {
        var registrationDetails = moturResponse.RegistrationResponse?.Details;
        var motDetails = moturResponse.MotResponse?.Details;

        var registrationPlate = registrationDetails?.RegistrationPlate ?? motDetails?.RegistrationPlate;
        var make = registrationDetails?.Make ?? motDetails?.Make ?? "Unknown";
        var model = motDetails?.Model ?? "Unknown";
        var colour = registrationDetails?.Colour ?? motDetails?.Colour ?? "Unknown";
        var latestMotPassed = motDetails?.MotTests?.OrderByDescending(x => x.CompletedDate)?.FirstOrDefault(x => x.TestResult == "PASSED");
        var motStatus = registrationDetails?.MotStatus ?? (latestMotPassed != null && latestMotPassed?.ExpiryDate > DateTime.UtcNow ? "Valid" : "Not Valid");
        var hasEngineSize = decimal.TryParse(registrationDetails?.EngineCapacityInCc.ToString() ?? motDetails?.EngineSize, out var engineSizeDecimal);
        var engineSize = hasEngineSize ? $"{decimal.Round((engineSizeDecimal) / 1000, 1)}L" : "Unknown";
        var fuelType = registrationDetails?.FuelType?.ToLower() ?? motDetails?.FuelType ?? "Unknown";
        var manufactureDate = motDetails?.ManufactureDate?.ToShortDateString() ?? registrationDetails?.YearOfManufacture?.ToString();

        var embed = new EmbedProperties();
        embed.WithTitle("Vehicle Information");

        Color embedColour;

        if ((motStatus == "Valid" || motDetails?.FirstMotTestDueDate > DateTime.UtcNow) && registrationDetails?.TaxStatus == "Taxed")
            embedColour = Green;
        else if (motStatus == "Valid" || motDetails?.FirstMotTestDueDate > DateTime.UtcNow && registrationDetails?.TaxStatus != "Taxed")
            embedColour = Orange;
        else if (moturResponse.RegistrationResponse?.ErrorDetails?.HttpStatusCode == 404 && moturResponse.MotResponse?.ErrorDetails == null)
        {
            embedColour = Black;
            embed.Footer = new EmbedFooterProperties().WithText("DVLA has no records for this vehicle, it has probably been scrapped");
        }
        else
            embedColour = Red;

        embed.WithColor(embedColour);
        embed.AddFields(new EmbedFieldProperties().WithName("Reg plate").WithValue(registrationPlate).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("MOT Status").WithValue(motStatus).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Tax Status").WithValue(registrationDetails?.TaxStatus ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Last Issued V5C Date").WithValue(registrationDetails?.DateOfLastV5cIssued?.ToShortDateString() ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Make").WithValue(make).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Model").WithValue(model).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Colour").WithValue(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(colour)).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Engine Size").WithValue(engineSize).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Fuel Type").WithValue(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fuelType)).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Wheelplan").WithValue(registrationDetails?.Wheelplan ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("CO2 Emissions (g/km)").WithValue(registrationDetails?.Co2EmissionsInGramPerKm?.ToString() ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Manufacture Date").WithValue(manufactureDate ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Weight in kg").WithValue(registrationDetails?.RevenueWeightInKg?.ToString() ?? "Unknown").WithInline());

        embed.WithDescription($"{VehicleMakeTaglineGenerator(make)}\n{VehicleModelTaglineGenerator(model)}");

        return embed;
    }

    private static string VehicleMakeTaglineGenerator(string make) => make switch
    {
        "JAGUAR" => "Jaaaaaaaaaaaaaag",
        "ROVER" => "It's a Rover... it won't turn over",
        "BMW" => "Up your arse in the outside lane",
        "HYUNDAI" => "High and dry",
        "LOTUS" => "Lots Of Trouble Usually Serious",
        "FIAT" => "Fix It Again Tony",
        _ => ""
    };

    private static string VehicleModelTaglineGenerator(string model) => model switch
    {
        "MONDEO" => "Mondildo",
        _ => ""
    };

    public static EmbedProperties BuildMotSummaryEmbed(List<MotTest> motTests)
    {
        var orderedMotTests = motTests.OrderByDescending(test => test.CompletedDate).ToList();

        var embed = new EmbedProperties();
        embed.WithTitle("MOT Information");
        embed.WithDescription("Summary for all MOT tests on this vehicle");

        foreach (var motTest in orderedMotTests.Take(5))
        {
            var distinctDefects = motTest.Defects.DistinctBy(x => x.Type).ToList();
            embed.AddFields(new EmbedFieldProperties()
                .WithName($"{motTest.CompletedDate.GetValueOrDefault():dd MMM yyyy} - {motTest.TestResult}")
                .WithValue($"{string.Join("\n", distinctDefects.Select(x => $" {motTest.Defects.Count(defect => defect.Type == x.Type)} x {x.Type.ToString()}"))}"));
        }

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Stats"));

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Number of MOT tests performed")
            .WithValue(motTests.Count.ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Passed tests")
            .WithValue(motTests.Count(test => test.TestResult == "PASSED").ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Failed tests")
            .WithValue(motTests.Count(test => test.TestResult == "FAILED").ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average number of advisories per year")
            .WithValue(
                motTests.Average(test => test.Defects.Count(defect => defect.Type == DefectType.ADVISORY)).ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average number of minor/major/dangerous per year")
            .WithValue(
                motTests.Average(test => test.Defects.Count(defect => defect.Type is
                    DefectType.DANGEROUS or
                    DefectType.MAJOR or
                    DefectType.MINOR or
                    DefectType.FAIL)).ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        var motTestDifferencesBetweenSuccessAndFailureCycles = new List<(int, TimeSpan)>();
        foreach (var motTest in orderedMotTests.Select((value, i) => new { value, i }).Where(test => test.value.TestResult == "PASSED"))
        {
            int? previousOdometerReadingInMiles = null;
            DateTime? previousCompletedDate = null;
            bool inFailureCycle = false;

            for (var index = motTest.i + 1; index < motTests.Count - 1; index++)
            {
                if (motTests.ElementAt(index).TestResult == "FAILED")
                {
                    inFailureCycle = true;
                    previousOdometerReadingInMiles = NormaliseOdometerValueToMiles(motTests.ElementAt(index).OdometerValue,
                        motTests.ElementAt(index).OdometerUnit);
                    previousCompletedDate = motTests.ElementAt(index).CompletedDate;
                }
                else
                    break;
            }

            var currentOdometerReadingInMiles =
                NormaliseOdometerValueToMiles(motTest.value.OdometerValue, motTest.value.OdometerUnit);
            var currentCompletedDate = motTest.value.CompletedDate;
            if (inFailureCycle && previousOdometerReadingInMiles.HasValue && previousCompletedDate.HasValue && currentOdometerReadingInMiles.HasValue && currentCompletedDate.HasValue)
            {
                motTestDifferencesBetweenSuccessAndFailureCycles.Add((currentOdometerReadingInMiles.Value - previousOdometerReadingInMiles.Value, currentCompletedDate.Value - previousCompletedDate.Value));
            }

        }

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average amount of days from a test failure until pass")
            .WithValue(motTestDifferencesBetweenSuccessAndFailureCycles.Any() ? motTestDifferencesBetweenSuccessAndFailureCycles.Average(dt => dt.Item2.Days).ToString(CultureInfo.InvariantCulture) : "0")
            .WithInline());

        return embed;
    }


    private static int? NormaliseOdometerValueToMiles(string? odometerValue, string? odometerUnit)
    {
        var parsed = int.TryParse(odometerValue, out var odometerValueInt);
        if (!parsed || string.IsNullOrWhiteSpace(odometerUnit)) return null;

        if (odometerUnit == "MI")
            return odometerValueInt;
        return (int)Math.Ceiling(odometerValueInt / 1.609);
    }

    public static EmbedProperties BuildMotTestEmbed(string? reg, MotTest motTest)
    {
        var embed = new EmbedProperties();
        embed.WithTitle($"MOT {motTest.CompletedDate}");


        var motEmbedColor = motTest.TestResult == "PASSED" && motTest.Defects.Count == 0 ? Green :
                                motTest.TestResult == "PASSED" ? Orange : Red;

        embed.WithColor(motEmbedColor);
        embed.WithTitle($"MOT {motTest.CompletedDate?.ToShortDateString()} - {motTest.TestResult}");
        embed.AddFields(new EmbedFieldProperties().WithName("Reg plate").WithValue(reg));
        if (motTest.TestResult == "PASSED")
            embed.AddFields(new EmbedFieldProperties().WithName("Expires").WithValue(motTest.ExpiryDate?.ToLongDateString()).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Odometer reading").WithValue($"{motTest.OdometerValue} {motTest.OdometerUnit}").WithInline());

        var groupedDefects = motTest.Defects
            .OrderBy(defect => defect.Type)
            .GroupBy(defect => defect.Type);
        foreach (var defectGroup in groupedDefects)
        {
            var embedFieldLengthLimit = 1024;
            var defectText = string.Join("\n", defectGroup.Select(x => $"- {x.Text}"));

            var defectChunks = defectGroup.Chunk(defectGroup.Count() / defectText.Chunk(embedFieldLengthLimit).Count());

            foreach (var chunk in defectChunks)
            {
                embed.AddFields(new EmbedFieldProperties().WithName(chunk.FirstOrDefault()?.Type.ToString()).WithValue(string.Join("\n", chunk.Select(x => $"- {x.Text}"))));
            }
        }
        return embed;
    }
}