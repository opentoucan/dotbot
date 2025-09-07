using System.Globalization;
using Dotbot.Api.Domain;
using Dotbot.Api.Dto.MotApi;
using NetCord;
using NetCord.Rest;

namespace Dotbot.Api.Discord;

public static class VehicleInformationAndMotEmbedBuilder
{
    private static readonly Color Green = new(0, 128, 0);
    private static readonly Color Orange = new(255, 140, 0);
    private static readonly Color Red = new(255, 0, 0);
    private static readonly Color Black = new(0, 0, 0);

    public static EmbedProperties BuildVehicleInformationEmbed(VehicleInformation vehicleInformation)
    {
        var embed = new EmbedProperties();

        embed.WithTitle("Vehicle Information");

        Color embedColour;

        if (vehicleInformation.MotStatus.IsValid || vehicleInformation.TaxStatus.IsValid)
        {
            embedColour = Green;
        }
        else if (vehicleInformation.PotentiallyScrapped)
        {
            embedColour = Black;
            embed.WithDescription("DVLA has no records for this vehicle, it has probably been scrapped");
        }
        else if (vehicleInformation.MotStatus.IsValid || !vehicleInformation.TaxStatus.IsValid)
        {
            embedColour = Orange;
        }
        else
        {
            embedColour = Red;
        }

        embed.WithColor(embedColour);
        embed.AddFields(new EmbedFieldProperties().WithName("Reg plate").WithValue(vehicleInformation.Registration)
            .WithInline());

        var motStatusText = vehicleInformation.MotStatus.IsExempt ? "Exempt" :
            vehicleInformation.MotStatus.IsValid ? "Valid" : "Expired";

        embed.AddFields(new EmbedFieldProperties().WithName("MOT Status").WithValue(motStatusText).WithInline());

        if (vehicleInformation.MotStatus.ValidUntil.HasValue &&
            DateTime.UtcNow < vehicleInformation.MotStatus.ValidUntil.Value)
            embed.AddFields(new EmbedFieldProperties().WithName("MOT Due Date")
                .WithValue(vehicleInformation.MotStatus.ValidUntil.Value.ToShortDateString()).WithInline());

        embed.AddFields(new EmbedFieldProperties().WithName("Tax Status")
            .WithValue(vehicleInformation.TaxStatus.IsExempt
                ? "Exempt"
                : vehicleInformation.TaxStatus.DvlaTaxStatusText)
            .WithInline());

        if (vehicleInformation.TaxStatus.TaxDueDate.HasValue)
            embed.AddFields(new EmbedFieldProperties().WithName("Tax Due Date")
                .WithValue(vehicleInformation.TaxStatus.TaxDueDate.GetValueOrDefault().ToShortDateString())
                .WithInline());

        if (vehicleInformation.LastIssuedV5CDate.HasValue)
            embed.AddFields(new EmbedFieldProperties().WithName("Last Issued V5C Date")
                .WithValue(vehicleInformation.LastIssuedV5CDate.GetValueOrDefault().ToShortDateString()).WithInline());

        if (!string.IsNullOrWhiteSpace(vehicleInformation.Make))
            embed.AddFields(new EmbedFieldProperties().WithName("Make").WithValue(vehicleInformation.Make)
                .WithInline());

        if (!string.IsNullOrWhiteSpace(vehicleInformation.Model))
            embed.AddFields(new EmbedFieldProperties().WithName("Model").WithValue(vehicleInformation.Model)
                .WithInline());

        if (!string.IsNullOrWhiteSpace(vehicleInformation.Colour))
            embed.AddFields(new EmbedFieldProperties().WithName("Colour")
                .WithValue(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(vehicleInformation.Colour)).WithInline());

        if (vehicleInformation.EngineCapacityLitres != null)
            embed.AddFields(new EmbedFieldProperties().WithName("Engine Size")
                .WithValue($"{vehicleInformation.EngineCapacityLitres}L")
                .WithInline());

        embed.AddFields(new EmbedFieldProperties().WithName("Fuel Type")
            .WithValue(vehicleInformation.FuelType.ToString()).WithInline());

        if (vehicleInformation.Co2InGramPerKilometer != null)
            embed.AddFields(new EmbedFieldProperties().WithName("CO2 Emissions (g/km)")
                .WithValue(vehicleInformation.Co2InGramPerKilometer.ToString()).WithInline());

        if (vehicleInformation.RegistrationDate.HasValue)
            embed.AddFields(new EmbedFieldProperties().WithName("Registration Date")
                .WithValue(vehicleInformation.RegistrationDate.GetValueOrDefault().ToShortDateString())
                .WithInline());

        if (vehicleInformation.WeightInKg != null)
            embed.AddFields(new EmbedFieldProperties().WithName("Weight")
                .WithValue($"{vehicleInformation.WeightInKg}kg").WithInline());

        embed.WithFooter(
            new EmbedFooterProperties().WithText(
                $"{VehicleMakeTaglineGenerator(vehicleInformation.Make)}\n{VehicleModelTaglineGenerator(vehicleInformation.Model)}"));

        return embed;
    }

    private static string VehicleMakeTaglineGenerator(string? make)
    {
        return make switch
        {
            "JAGUAR" => "Jaaaaaaaaaaaaaag",
            "ROVER" => "It's a Rover... it won't turn over",
            "BMW" => "Up your arse in the outside lane",
            "HYUNDAI" => "High and dry",
            "LOTUS" => "Lots Of Trouble Usually Serious",
            "FIAT" => "Fix It Again Tony",
            _ => ""
        };
    }

    private static string VehicleModelTaglineGenerator(string? model)
    {
        return model switch
        {
            "MONDEO" => "Mondildo",
            _ => ""
        };
    }

    public static EmbedProperties BuildMotSummaryEmbed(VehicleInformation vehicleInformation)
    {
        var motTests = vehicleInformation.VehicleMotTests.OrderByDescending(x => x.CompletedDate).ToList();
        var latestMot = motTests.FirstOrDefault();
        var embed = new EmbedProperties();
        var embedColour =
            latestMot?.Result == MotTestResult.PASSED && latestMot.Defects.Count == 0 &&
            vehicleInformation.MotStatus.IsValid ? Green :
            latestMot?.Result == MotTestResult.PASSED ? Orange : Red;
        embed.WithColor(embedColour);
        embed.WithTitle("MOT Information");
        embed.WithDescription("Summary for all MOT tests on this vehicle");

        if (motTests.Count == 0)
        {
            embed.WithDescription("No MOTs Found");
            if (vehicleInformation.MotStatus.ValidUntil.HasValue)
                embed.AddFields(new EmbedFieldProperties().WithName("First MOT Due Date")
                    .WithValue(vehicleInformation.MotStatus.ValidUntil.GetValueOrDefault().ToShortDateString()));
            return embed;
        }

        foreach (var motTest in motTests.Take(5))
        {
            var distinctDefects = motTest.Defects.DistinctBy(x => x.DefectType).ToList();
            embed.AddFields(new EmbedFieldProperties()
                .WithName($"{motTest.CompletedDate:dd MMM yyyy} - {motTest.Result}")
                .WithValue(
                    $"{string.Join("\n", distinctDefects.Select(x => $" {motTest.Defects.Count(defect => defect.DefectType == x.DefectType)} x {x.DefectType}"))}"));
        }

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Stats"));

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Latest odometer reading")
            .WithValue($"{motTests.First().OdometerReadingInMiles} Miles"));

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Number of MOT tests performed")
            .WithValue(motTests.Count.ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Passed tests")
            .WithValue(motTests.Count(test => test.Result == MotTestResult.PASSED).ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Failed tests")
            .WithValue(motTests.Count(test => test.Result == MotTestResult.FAILED).ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average number of advisories per year")
            .WithValue(
                motTests.Average(test =>
                        test.Defects.Count(defect =>
                            defect.DefectType == VehicleMotTest.Defect.Type.ADVISORY))
                    .ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average number of minor/major/dangerous per year")
            .WithValue(
                motTests.Average(test => test.Defects.Count(defect => defect.DefectType is
                    VehicleMotTest.Defect.Type.DANGEROUS or
                    VehicleMotTest.Defect.Type.MAJOR or
                    VehicleMotTest.Defect.Type.MINOR or
                    VehicleMotTest.Defect.Type.FAIL)).ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        var motTestDifferencesBetweenSuccessAndFailureCycles = new List<(int, TimeSpan?)>();
        foreach (var motTest in motTests.Select((value, i) => new { value, i })
                     .Where(test => test.value.Result == MotTestResult.PASSED))
        {
            int? previousOdometerReadingInMiles = null;
            DateTime? previousCompletedDate = null;
            var inFailureCycle = false;

            for (var index = motTest.i + 1; index < motTests.Count - 1; index++)
                if (motTests.ElementAt(index).Result == MotTestResult.PASSED)
                {
                    break;
                }
                else
                {
                    inFailureCycle = true;
                    previousOdometerReadingInMiles = motTests.ElementAt(index).OdometerReadingInMiles;
                    previousCompletedDate = motTests.ElementAt(index).CompletedDate;
                }

            var currentOdometerReadingInMiles = motTest.value.OdometerReadingInMiles;
            var currentCompletedDate = motTest.value.CompletedDate;
            if (inFailureCycle && previousOdometerReadingInMiles.HasValue && previousCompletedDate.HasValue &&
                currentOdometerReadingInMiles.HasValue)
                motTestDifferencesBetweenSuccessAndFailureCycles.Add((
                    currentOdometerReadingInMiles.Value - previousOdometerReadingInMiles.Value,
                    currentCompletedDate - previousCompletedDate.Value));
        }

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average amount of days from a test failure until pass")
            .WithValue(motTestDifferencesBetweenSuccessAndFailureCycles.Count > 0
                ? motTestDifferencesBetweenSuccessAndFailureCycles.Average(dt => dt.Item2?.Days)
                    .ToString()
                : "0")
            .WithInline());

        return embed;
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
            embed.AddFields(new EmbedFieldProperties().WithName("Expires")
                .WithValue(motTest.ExpiryDate?.ToLongDateString()).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Odometer reading")
            .WithValue($"{motTest.OdometerValue} {motTest.OdometerUnit}").WithInline());

        var groupedDefects = motTest.Defects
            .OrderBy(defect => defect.Type)
            .GroupBy(defect => defect.Type);
        foreach (var defectGroup in groupedDefects)
        {
            var embedFieldLengthLimit = 1024;
            var defectText = string.Join("\n", defectGroup.Select(x => $"- {x.Text}"));

            var defectChunks = defectGroup.Chunk(defectGroup.Count() / defectText.Chunk(embedFieldLengthLimit).Count());

            foreach (var chunk in defectChunks)
                embed.AddFields(new EmbedFieldProperties().WithName(chunk.FirstOrDefault()?.Type)
                    .WithValue(string.Join("\n", chunk.Select(x => $"- {x.Text}"))));
        }

        return embed;
    }
}