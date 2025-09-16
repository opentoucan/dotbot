using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Dotbot.Infrastructure.Entities.Reports;

public partial class VehicleMotTestDefect : Entity
{
    [JsonConstructor]
    private VehicleMotTestDefect(MotDefectCategory category,
        VehicleMotInspectionDefectDefinition defectDefinition,
        bool isDangerous)
    {
        Category = category;
        DefectDefinition = defectDefinition;
        IsDangerous = isDangerous;
    }

    public VehicleMotTestDefect(MotDefectCategory category,
        bool isDangerous)
    {
        Category = category;
        IsDangerous = isDangerous;
    }

    public VehicleMotTestDefect(string? categoryText, VehicleMotInspectionDefectDefinition defectDefinition,
        bool? dangerous)
    {
        if (!Enum.TryParse(WhitespaceRegex().Replace(categoryText!, "").ToUpper(), out MotDefectCategory category))
            throw new ArgumentException($"Invalid MOT Defect type: {categoryText}");
        Category = category;
        DefectDefinition = defectDefinition;
        IsDangerous = Category == MotDefectCategory.DANGEROUS || dangerous.GetValueOrDefault();
    }

    public MotDefectCategory Category { get; set; }
    public VehicleMotInspectionDefectDefinition DefectDefinition { get; set; } = null!;
    public bool IsDangerous { get; set; }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}