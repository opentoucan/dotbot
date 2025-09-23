using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Entities.Reports;
using Dotbot.Infrastructure.EntityConfigurations;
using Dotbot.Infrastructure.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Dotbot.Infrastructure;

public class DotbotContext : DbContext, IUnitOfWork
{
    public DotbotContext(DbContextOptions<DotbotContext> options) : base(options)
    {
    }

    public DbSet<CustomCommand> CustomCommands { get; set; } = null!;
    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<CommandAttachment> Attachments { get; set; } = null!;
    public DbSet<Xkcd> Xkcds { get; set; } = null!;
    public DbSet<DiscordCommandLog> DiscordCommandLogs { get; set; } = null!;
    public DbSet<VehicleInformation> VehicleInformation { get; set; } = null!;
    public DbSet<VehicleMotInspectionDefectDefinition> MotInspectionDefectDefinitions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dotbot");
        modelBuilder.ApplyConfiguration(new CustomCommandEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CommandAttachmentEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new XkcdEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new VehicleInformationEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleMotTestEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleMotTestDefectEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleMotInspectionDefectDefinitionEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DiscordCommandLogEntityTypeConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetConverter>();
    }
}