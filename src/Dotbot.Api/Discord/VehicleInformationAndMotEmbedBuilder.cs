using System.Globalization;
using Ardalis.Result;
using Dotbot.Api.Dto;
using Dotbot.Api.Dto.MotApi;
using Dotbot.Api.Dto.VesApi;
using NetCord;
using NetCord.Rest;

namespace Dotbot.Api.Discord;

public static class VehicleInformationAndMotEmbedBuilder
{
    private static readonly Color Green = new(0, 128, 0);
    private static readonly Color Orange = new(255, 140, 0);
    private static readonly Color Red = new(255, 0, 0);
    private static readonly Color Black = new(0, 0, 0);
    public static EmbedProperties BuildVehicleInformationEmbed(Result<VesApiResponse> vesApiResponse, Result<MotApiResponse> motApiResponse)
    {
        var embed = new EmbedProperties();
        
        if (!vesApiResponse.IsSuccess || vesApiResponse.Value == null)
        {
            embed.WithTitle("Failed to retrieve vehicle information");
            embed.WithDescription(string.Join("\n", vesApiResponse.Errors));
            return embed;
        }
        
        var vehicleRegistrationData = vesApiResponse.Value!;
        var registrationPlate = vehicleRegistrationData.RegistrationNumber;
        var make = vehicleRegistrationData.Make ?? "Unknown";
        var model = motApiResponse.Value?.Model ?? "Unknown";
        var colour = vehicleRegistrationData.Colour ?? motApiResponse.Value?.PrimaryColour ?? "Unknown";
        var latestMotPassed = motApiResponse.Value?.MotTests?.OrderByDescending(x => x.CompletedDate)?.FirstOrDefault(x => x.TestResult == "PASSED");
        var motStatus = vehicleRegistrationData.MotStatus ?? (latestMotPassed != null && latestMotPassed?.ExpiryDate > DateTime.UtcNow ? "Valid" : "Not Valid");
        var hasEngineSize = decimal.TryParse(vehicleRegistrationData.EngineCapacity.ToString() ?? motApiResponse.Value?.EngineSize, out var engineSizeDecimal);
        var engineSize = hasEngineSize ? $"{decimal.Round((engineSizeDecimal) / 1000, 1)}L" : "Unknown";
        var fuelType = vehicleRegistrationData.FuelType?.ToLower() ?? motApiResponse.Value?.FuelType ?? "Unknown";
        var manufactureDate = motApiResponse.Value?.ManufactureDate?.ToShortDateString() ?? vehicleRegistrationData.YearOfManufacture?.ToString();
        
        embed.WithTitle("Vehicle Information");

        Color embedColour;

        if ((motStatus == "Valid" || motApiResponse.Value?.MotTestDueDate > DateTime.UtcNow) && vehicleRegistrationData.TaxStatus == "Taxed")
            embedColour = Green;
        else if (motStatus == "Valid" || motApiResponse.Value?.MotTestDueDate > DateTime.UtcNow && vehicleRegistrationData.TaxStatus != "Taxed")
            embedColour = Orange;
        else if (vesApiResponse.Value.Errors?.FirstOrDefault()?.Code == "404" && motApiResponse.Value == null)
        {
            embedColour = Black;
            embed.WithDescription("DVLA has no records for this vehicle, it has probably been scrapped");
        }
        else
            embedColour = Red;

        embed.WithColor(embedColour);
        embed.AddFields(new EmbedFieldProperties().WithName("Reg plate").WithValue(registrationPlate).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("MOT Status").WithValue(motStatus).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Tax Status").WithValue(vehicleRegistrationData.TaxStatus ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Last Issued V5C Date").WithValue(vehicleRegistrationData.DateOfLastV5cIssued?.ToShortDateString() ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Make").WithValue(make).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Model").WithValue(model).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Colour").WithValue(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(colour)).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Engine Size").WithValue(engineSize).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Fuel Type").WithValue(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fuelType)).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Wheelplan").WithValue(vehicleRegistrationData.Wheelplan ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("CO2 Emissions (g/km)").WithValue(vehicleRegistrationData.Co2Emissions?.ToString() ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Manufacture Date").WithValue(manufactureDate ?? "Unknown").WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Weight in kg").WithValue(vehicleRegistrationData.RevenueWeight?.ToString() ?? "Unknown").WithInline());

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

    public static EmbedProperties BuildMotSummaryEmbed(Result<MotApiResponse> motApiResponse)
    {
        if (!motApiResponse.IsSuccess || motApiResponse.Value == null)
        {
            return new  EmbedProperties().WithTitle("Failed to retrieve MOT information").WithDescription(string.Join("\n", motApiResponse.Errors));
        }
        
        var motTests = motApiResponse.Value.MotTests.OrderByDescending(x => x.CompletedDate).ToList();

        var embed = new EmbedProperties();
        embed.WithTitle("MOT Information");
        embed.WithDescription("Summary for all MOT tests on this vehicle");

        if (motTests.Count == 0)
        {
            embed.WithDescription("No MOTs Found");
            return embed;
        }
        foreach (var motTest in motTests.Take(5))
        {
            var distinctDefects = motTest.Defects.DistinctBy(x => x.Type).ToList();
            embed.AddFields(new EmbedFieldProperties()
                .WithName($"{motTest.CompletedDate.GetValueOrDefault():dd MMM yyyy} - {motTest.TestResult}")
                .WithValue($"{string.Join("\n", distinctDefects.Select(x => $" {motTest.Defects.Count(defect => defect.Type == x.Type)} x {x.Type.ToString()}"))}"));
        }

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Stats"));

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Latest odometer reading")
            .WithValue($"{motTests.First().OdometerValue} {motTests.First().OdometerUnit}"));

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
                motTests.Average(test => test.Defects.Count(defect => defect.Type == MotApiResponse.MotTest.Defect.DefectType.ADVISORY)).ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average number of minor/major/dangerous per year")
            .WithValue(
                motTests.Average(test => test.Defects.Count(defect => defect.Type is
                    MotApiResponse.MotTest.Defect.DefectType.DANGEROUS or
                    MotApiResponse.MotTest.Defect.DefectType.MAJOR or
                    MotApiResponse.MotTest.Defect.DefectType.MINOR or
                    MotApiResponse.MotTest.Defect.DefectType.FAIL)).ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        var motTestDifferencesBetweenSuccessAndFailureCycles = new List<(int, TimeSpan)>();
        foreach (var motTest in motTests.Select((value, i) => new { value, i }).Where(test => test.value.TestResult == "PASSED"))
        {
            int? previousOdometerReadingInMiles = null;
            DateTime? previousCompletedDate = null;
            var inFailureCycle = false;

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
            .WithValue(motTestDifferencesBetweenSuccessAndFailureCycles.Count > 0 ? motTestDifferencesBetweenSuccessAndFailureCycles.Average(dt => dt.Item2.Days).ToString(CultureInfo.InvariantCulture) : "0")
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

    public static EmbedProperties BuildMotTestEmbed(string? reg, MotApiResponse.MotTest motTest)
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