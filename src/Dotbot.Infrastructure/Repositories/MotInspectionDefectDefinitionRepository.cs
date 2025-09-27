using System.Text.RegularExpressions;
using Dotbot.Infrastructure.Entities.Reports;

namespace Dotbot.Infrastructure.Repositories;

public interface IMotInspectionDefectDefinitionRepository
{
    VehicleMotInspectionDefectDefinition GetByDefectDescription(string defectDescription);
}

public partial class MotInspectionDefectDefinitionRepository(DotbotContext dbContext)
    : IMotInspectionDefectDefinitionRepository
{
    public VehicleMotInspectionDefectDefinition GetByDefectDescription(string defectDescription)
    {
        var matchingGroups = CategoryHeading()
            .Match(defectDescription).Groups;
        var defectReferenceCode = WhitespaceRegex().Replace(matchingGroups["reference"].Value, "");
        var categoryNumber = WhitespaceRegex().Replace(matchingGroups["category"].Value, "");
        var vehicleMotInspectionDefectDefinition = dbContext.MotInspectionDefectDefinitions.FirstOrDefault(x =>
            x.DefectReferenceCode == defectReferenceCode
            && ((x.SubCategoryName ?? string.Empty).StartsWith(categoryNumber) ||
                (x.CategoryArea ?? string.Empty).StartsWith(categoryNumber)));
        if (vehicleMotInspectionDefectDefinition is not null)
            return vehicleMotInspectionDefectDefinition;

        var topLevelCategory = DefectContainsReferenceCode().Match(defectDescription).Success
            ? "Old definition not in latest manual"
            : "Manually entered defect by tester";
        return new VehicleMotInspectionDefectDefinition
        {
            Id = Guid.NewGuid(),
            TopLevelCategory = topLevelCategory,
            DefectName = defectDescription
        };
    }

    [GeneratedRegex("\\s")]
    private static partial Regex WhitespaceRegex();

    // Looks for a reference code like (4.1.E.1) in the defect
    [GeneratedRegex("\\(([0-9a-zA-Z+](\\.|\\)))+")]
    private static partial Regex DefectContainsReferenceCode();

    // Looks for a category heading and captures the reference code and title separately like (1.1 Condition and Operation) or (1.1.7. Brake valves)
    [GeneratedRegex("\\((?<category>(?:[0-9]|\\.)+)(?<reference>(?:\\s?\\(\\w+\\)\\s?)+)")]
    private static partial Regex CategoryHeading();
}