namespace Dotbot.Infrastructure.Entities.Reports;

public class VehicleMotInspectionDefectDefinition : Entity
{
    public required string TopLevelCategory { get; set; }
    public string? CategoryArea { get; set; }
    public string? SubCategoryName { get; set; }
    public string? DefectReferenceCode { get; set; }
    public required string DefectName { get; set; }
}