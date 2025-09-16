using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class VehicleMotTestDefectEntityTypeConfiguration : IEntityTypeConfiguration<VehicleMotTestDefect>
{
    public void Configure(EntityTypeBuilder<VehicleMotTestDefect> builder)
    {
        builder.ToTable("vehicle_mot_test_defect", "reporting");

        builder.HasKey(mtd => mtd.Id);
        builder.HasOne(defect => defect.DefectDefinition);
    }
}