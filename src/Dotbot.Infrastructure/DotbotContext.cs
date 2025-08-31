using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.EntityConfigurations;
using Dotbot.Infrastructure.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dotbot.Infrastructure;

public class DotbotContext : DbContext, IUnitOfWork
{
    public DbSet<CustomCommand> CustomCommands { get; set; } = null!;
    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<CommandAttachment> Attachments { get; set; } = null!;
    public DbSet<Xkcd> Xkcds { get; set; } = null!;


    private IDbContextTransaction? _currentTransaction = null!;

    public DotbotContext(DbContextOptions<DotbotContext> options) : base(options) { }

    public bool HasActiveTransaction => _currentTransaction != null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dotbot");
        modelBuilder.ApplyConfiguration(new CustomCommandEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CommandAttachmentEntityTypeConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetConverter>();
    }
}