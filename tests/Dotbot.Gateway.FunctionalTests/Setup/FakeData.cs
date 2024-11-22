using Bogus;
using Discord;
using Dotbot.Gateway.Dto.Requests.Discord;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities;
using Guild = Dotbot.Infrastructure.Entities.Guild;

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

public class AttachmentFaker : Faker<Dto.Requests.Discord.Attachment>
{
    public AttachmentFaker() =>
        UseSeed(69)
            .RuleFor(a => a.Id, f => f.Random.Guid().ToString())
            .RuleFor(a => a.Filename, f => f.Random.Word() + f.System.FileType())
            .RuleFor(a => a.Url, f => f.Image.PlaceImgUrl(f.Random.Int(), f.Random.Int()) + "." + f.System.FileExt())
            .RuleFor(a => a.ContentType, f => f.System.MimeType())
            .RuleFor(a => a.Size, f => f.Random.UShort());
}

public class ResolvedFaker : Faker<Resolved>
{
    public ResolvedFaker() =>
        UseSeed(69)
            .RuleFor(resolved => resolved.Attachments, f => new AttachmentFaker().Generate(10).ToDictionary(a => f.Random.ULong(), a => a));
}

public class CustomCommandRequestFaker : Faker<Data>
{
    public CustomCommandRequestFaker() =>
        UseSeed(69)
            .RuleFor(data => data.Name, _ => "save")
            .RuleFor(data => data.Options,f =>
            [
                new()
                {
                    SubOptions =
                    [
                        new() { Name = "name", Value = f.Lorem.Word() },
                        new() { Name = "text", Value = f.Lorem.Text() }
                    ]
                }
            ])
            .RuleFor(data => data.Resolved, new ResolvedFaker().Generate());
}
public class InteractionRequestFaker : Faker<InteractionRequest>
{
    public InteractionRequestFaker(Faker<Data> commandFaker, string? guildId = null) =>
        UseSeed(69)
            .RuleFor(req => req.Type, _ => (int)InteractionType.ApplicationCommand)
            .RuleFor(req => req.Guild,
                f => new Dto.Requests.Discord.Guild { Id = guildId ?? f.Random.Guid().ToString(), Features = [] })
            .RuleFor(req => req.Member,
                f => new Member { Roles = new List<string>(), User = new User { Id = f.Random.UInt().ToString() } })
            .RuleFor(req => req.Data, _ => commandFaker.Generate());
}