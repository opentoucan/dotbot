using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class VehicleInformationEntityTypeConfiguration : IEntityTypeConfiguration<VehicleInformation>
{
    public void Configure(EntityTypeBuilder<VehicleInformation> builder)
    {
        builder.ToTable("vehicle_information", "reporting");
        builder.HasKey(vi => vi.Id);

        builder.OwnsOne(vi => vi.TaxStatus);
        builder.OwnsOne(vi => vi.MotStatus);

        builder.Property(vi => vi.Registration).IsRequired();
        builder.HasIndex(vi => vi.Registration).IsUnique();

        builder.HasMany(vi => vi.VehicleMotTests).WithOne();
    }
}