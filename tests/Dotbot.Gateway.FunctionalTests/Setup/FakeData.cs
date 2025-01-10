using Bogus;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities;

namespace Dotbot.Gateway.FunctionalTests.Setup;

public static class FakeData
{
    public static void PopulateTestData(DotbotContext dbContext, string? guildId = null)
    {
        var guild = new Faker<Guild>()
            .UseSeed(69)
            .CustomInstantiator(f => new Guild(guildId ?? new Faker().Random.ULong().ToString(), f.Random.Word()))
            .RuleFor(g => g.CustomCommands, _ => new Faker<CustomCommand>()
                .UseSeed(69)
                .CustomInstantiator(f => new CustomCommand(f.Lorem.Word(), f.Random.UInt().ToString(), f.Random.Bool() ? f.Lorem.Sentence() : null))
                .RuleFor(c => c.Id, f => f.Random.Guid())
                .RuleFor(cc => cc.Attachments, _ => new Faker<CommandAttachment>()
                    .UseSeed(69)
                    .CustomInstantiator(f => new CommandAttachment(f.Lorem.Word(), f.System.FileType(), f.Internet.Url()))
                    .Generate(5))
                .Generate(10))
            .Generate();
        dbContext.Guilds.AddRange(guild);
        dbContext.SaveChanges();
    }
}