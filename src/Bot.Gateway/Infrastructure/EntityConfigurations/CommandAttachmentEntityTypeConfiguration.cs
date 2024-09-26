using Bot.Gateway.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bot.Gateway.Infrastructure.EntityConfigurations;

public class CommandAttachmentEntityTypeConfiguration : IEntityTypeConfiguration<CommandAttachment>
{
    public void Configure(EntityTypeBuilder<CommandAttachment> attachmentConfiguration)
    {
        attachmentConfiguration.ToTable("command_attachments");

        attachmentConfiguration.Property<Guid>("CustomCommandId");
    }
}