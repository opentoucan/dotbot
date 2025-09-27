using Dotbot.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class CommandAttachmentEntityTypeConfiguration : IEntityTypeConfiguration<CommandAttachment>
{
    public void Configure(EntityTypeBuilder<CommandAttachment> attachmentConfiguration)
    {
    }
}