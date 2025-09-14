using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class VehicleMotTestEntityTypeConfiguration : IEntityTypeConfiguration<VehicleMotTest>
{
    public void Configure(EntityTypeBuilder<VehicleMotTest> builder)
    {
        builder.ToTable("vehicle_mot_test", "reporting");

        builder.HasKey(vmt => vmt.Id);
        builder.Property(vmt => vmt.TestNumber).HasMaxLength(12);
        builder.HasIndex(vmt => vmt.TestNumber).IsUnique();
        builder.HasMany(vmt => vmt.Defects).WithOne();
    }
}