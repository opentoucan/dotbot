using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class
    VehicleMotInspectionDefectDefinitionEntityTypeConfiguration : IEntityTypeConfiguration<
    VehicleMotInspectionDefectDefinition>
{
    public void Configure(EntityTypeBuilder<VehicleMotInspectionDefectDefinition> builder)
    {
        builder.HasKey(md => md.Id);
        builder.HasIndex(i => new
            { i.TopLevelCategory, i.CategoryArea, i.SubCategoryName, i.DefectName, i.DefectReferenceCode }).IsUnique();
    }
}