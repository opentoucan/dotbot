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
        builder.ToTable("vehicle_mot_inspection_defect_definitions", "reporting");
        builder.HasKey(md => md.Id);
    }
}