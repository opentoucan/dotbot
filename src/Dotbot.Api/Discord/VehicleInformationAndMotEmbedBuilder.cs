using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.OpenApi.Extensions;
using NetCord;
using NetCord.Rest;
using VehicleMotTest = Dotbot.Infrastructure.Entities.Reports.VehicleMotTest;

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
        embed.AddFields(new EmbedFieldProperties().WithName("Reg plate")
            .WithValue(vehicleInformation.Registration)
            .WithInline());

        var motStatusText = vehicleInformation.MotStatus.IsExempt ? "Exempt" :
            vehicleInformation.MotStatus.IsValid ? "Valid" : "Expired";

        embed.AddFields(new EmbedFieldProperties().WithName("MOT Status").WithValue(motStatusText).WithInline());

        if (vehicleInformation.MotStatus.ValidUntil.HasValue &&
            DateTime.UtcNow < vehicleInformation.MotStatus.ValidUntil.Value)
            embed.AddFields(new EmbedFieldProperties().WithName("MOT Due Date")
                .WithValue(vehicleInformation.MotStatus.ValidUntil.Value.DateTime.ToShortDateString())
                .WithInline());

        embed.AddFields(new EmbedFieldProperties().WithName("Tax Status")
            .WithValue(vehicleInformation.TaxStatus.IsExempt
                ? "Exempt"
                : vehicleInformation.TaxStatus.DvlaTaxStatusText)
            .WithInline());

        if (vehicleInformation.TaxStatus.TaxDueDate.HasValue)
            embed.AddFields(new EmbedFieldProperties().WithName("Tax Due Date")
                .WithValue(vehicleInformation.TaxStatus.TaxDueDate.GetValueOrDefault().DateTime
                    .ToShortDateString())
                .WithInline());

        if (vehicleInformation.LastIssuedV5CDate.HasValue)
            embed.AddFields(new EmbedFieldProperties().WithName("Last Issued V5C Date")
                .WithValue(vehicleInformation.LastIssuedV5CDate.GetValueOrDefault().DateTime
                    .ToShortDateString())
                .WithInline());

        if (!string.IsNullOrWhiteSpace(vehicleInformation.Make))
            embed.AddFields(new EmbedFieldProperties().WithName("Make").WithValue(vehicleInformation.Make)
                .WithInline());

        if (!string.IsNullOrWhiteSpace(vehicleInformation.Model))
            embed.AddFields(new EmbedFieldProperties().WithName("Model").WithValue(vehicleInformation.Model)
                .WithInline());

        if (!string.IsNullOrWhiteSpace(vehicleInformation.Colour))
            embed.AddFields(new EmbedFieldProperties().WithName("Colour")
                .WithValue(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(vehicleInformation.Colour))
                .WithInline());

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
                .WithValue(
                    vehicleInformation.RegistrationDate.GetValueOrDefault().DateTime.ToShortDateString())
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
        var embed = new EmbedProperties();

        var motTests = vehicleInformation.VehicleMotTests.OrderByDescending(x => x.CompletedDate).ToList();
        var latestMot = motTests.FirstOrDefault();

        var embedColour =
            latestMot?.Result == TestResult.PASSED && latestMot.Defects.Count == 0 &&
            vehicleInformation.MotStatus.IsValid ? Green :
            latestMot?.Result == TestResult.PASSED ? Orange : Red;
        embed.WithColor(embedColour);
        embed.WithTitle("MOT Information");
        embed.WithDescription("Summary for all MOT tests on this vehicle");


        foreach (var motTest in motTests.Take(5))
        {
            var distinctDefects = motTest.Defects.DistinctBy(x => x.Category).ToList();
            embed.AddFields(new EmbedFieldProperties()
                .WithName($"{motTest.CompletedDate:dd MMM yyyy} - {motTest.Result}")
                .WithValue(
                    $"{string.Join("\n", distinctDefects.Select(x => $" {motTest.Defects.Count(defect => defect.Category == x.Category)} x {x.Category.GetAttributeOfType<DisplayAttribute>().Name}"))}"));
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
            .WithValue(motTests.Count(test => test.Result == TestResult.PASSED).ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Failed tests")
            .WithValue(motTests.Count(test => test.Result == TestResult.FAILED).ToString())
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average number of advisories per year")
            .WithValue(
                motTests.Average(test =>
                        test.Defects.Count(defect =>
                            defect.Category == MotDefectCategory.ADVISORY))
                    .ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        embed.AddFields(new EmbedFieldProperties()
            .WithName("Average number of minor/major/dangerous per year")
            .WithValue(
                motTests.Average(test => test.Defects.Count(defect => defect.Category is
                    MotDefectCategory.DANGEROUS or
                    MotDefectCategory.MAJOR or
                    MotDefectCategory.MINOR or
                    MotDefectCategory.FAIL)).ToString("F", CultureInfo.InvariantCulture))
            .WithInline());

        var motTestDifferencesBetweenSuccessAndFailureCycles = new List<(int, TimeSpan?)>();
        foreach (var motTest in motTests.Select((value, i) => new { value, i })
                     .Where(test => test.value.Result == TestResult.PASSED))
        {
            int? previousOdometerReadingInMiles = null;
            DateTimeOffset? previousCompletedDate = null;
            var inFailureCycle = false;

            for (var index = motTest.i + 1; index < motTests.Count - 1; index++)
                if (motTests.ElementAt(index).Result == TestResult.PASSED)
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

    public static EmbedProperties BuildMotTestEmbed(string? reg, VehicleMotTest motTest)
    {
        var embed = new EmbedProperties();
        embed.WithTitle($"MOT {motTest.CompletedDate}");


        var motEmbedColor = motTest.Result == TestResult.PASSED && motTest.Defects.Count == 0 ? Green :
            motTest.Result == TestResult.PASSED ? Orange : Red;

        embed.WithColor(motEmbedColor);
        embed.WithTitle($"MOT {motTest.CompletedDate?.DateTime.ToShortDateString()} - {motTest.Result}");
        embed.AddFields(new EmbedFieldProperties().WithName("Reg plate").WithValue(reg));
        if (motTest.Result == TestResult.PASSED)
            embed.AddFields(new EmbedFieldProperties().WithName("Expires")
                .WithValue(motTest.ExpiryDate?.DateTime.ToLongDateString()).WithInline());
        embed.AddFields(new EmbedFieldProperties().WithName("Odometer reading")
            .WithValue($"{motTest.OdometerReadingInMiles} Miles").WithInline());

        var groupedDefects = motTest.Defects
            .OrderBy(defect => defect.Category)
            .GroupBy(defect => defect.Category);
        foreach (var defectGroup in groupedDefects)
        {
            var embedFieldLengthLimit = 1024;
            var defectText = string.Join("\n", defectGroup.Select(x => $"- {x.DefectDefinition.DefectName}"));

            var defectChunks = defectGroup.Chunk(defectGroup.Count() / defectText.Chunk(embedFieldLengthLimit).Count());

            foreach (var chunk in defectChunks)
                embed.AddFields(new EmbedFieldProperties()
                    .WithName(chunk.FirstOrDefault()?.Category.GetAttributeOfType<DisplayAttribute>().Name)
                    .WithValue(string.Join("\n", chunk.Select(x => $"- {x.DefectDefinition.DefectName}"))));
        }

        return embed;
    }
}