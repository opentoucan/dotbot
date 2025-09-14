using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class VehicleCommandLogEntityTypeConfiguration : IEntityTypeConfiguration<VehicleCommandLog>
{
    public void Configure(EntityTypeBuilder<VehicleCommandLog> builder)
    {
        builder.ToTable("vehicle_command_log", "reporting");
        builder.HasKey(mc => mc.Id);
    }
}