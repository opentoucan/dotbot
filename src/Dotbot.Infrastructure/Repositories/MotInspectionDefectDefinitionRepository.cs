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
        var matchingGroups = new Regex("\\((?<category>(?:[0-9]|\\.)+)(?<reference>(?:\\s?\\(\\w+\\)\\s?)+)")
            .Match(defectDescription).Groups;
        var defectReferenceCode = WhitespaceRegex().Replace(matchingGroups["reference"].Value, "");
        var categoryNumber = WhitespaceRegex().Replace(matchingGroups["category"].Value, "");
        return dbContext.MotInspectionDefectDefinitions.FirstOrDefault(x =>
                   x.DefectReferenceCode == defectReferenceCode
                   && ((x.SubCategoryName ?? string.Empty).StartsWith(categoryNumber) ||
                       (x.CategoryArea ?? string.Empty).StartsWith(categoryNumber))) ??
               new VehicleMotInspectionDefectDefinition
               {
                   Id = Guid.NewGuid(),
                   TopLevelCategory = "Custom Defined",
                   DefectName = defectDescription
               };
    }

    [GeneratedRegex("\\s")]
    private static partial Regex WhitespaceRegex();
}