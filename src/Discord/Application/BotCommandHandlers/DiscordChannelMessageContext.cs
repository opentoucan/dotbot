using Discord.Application.Models;
using Discord.Entities;
using Discord.Extensions;
using Discord.WebSocket;

namespace Discord.BotCommandHandlers;

internal interface IDiscordChannelMessageContext : IServiceContext
{
    internal IGuild? GetGuild();
    internal IVoiceState? GetUserVoiceState();
    IMessageChannel? GetChannel();
}

public class DiscordChannelMessageContext : IDiscordChannelMessageContext
{
    private readonly SocketMessage _message;
    private readonly DiscordServer? _guild;
    
    public DiscordChannelMessageContext(SocketMessage message)
    {
        _message = message;
    }    
    
    public DiscordChannelMessageContext(SocketMessage message, DiscordServer? guild)
    {
        _message = message;
        _guild = guild;
    }

    public Privilege GetPrivilege()
    {
        if (_guild is null) return Privilege.Base;

        return _guild.ModeratorIds.Contains(_message.Author.Id.ToString()) ? Privilege.Moderator : Privilege.Base;
    }

    public async Task ReplyAsync(string msg)
    {
        var msgRef = new MessageReference(_message.Id);
        await _message.Channel.SendMessageAsync(msg, false, null, RequestOptions.Default, AllowedMentions.All, msgRef);
    }

    public async Task SendMessageAsync(string msg)
    {
        await _message.Channel.SendMessageAsync(msg, false, null, RequestOptions.Default, AllowedMentions.All);
    }

    public async Task SendFileAsync(string fileName, Stream fs)
    {
        await _message.Channel.SendFileAsync(fs, fileName);
    }

    public async Task<string> GetServerId()
    {
        return _message.Channel.AsGuildChannel()?.Guild.Id.ToString();
    }

    public async Task<string> GetChannelId()
    {
        return _message.Channel.Id.ToString();
    }

    public async Task<bool> HasAttachments()
    {
        return _message.Attachments.Any();
    }

    public async Task<IReadOnlyCollection<MessageAttachment>> GetAttachments()
    {
        return _message.Attachments.Select(Convert).ToList();
    }

    public async Task SendFormattedMessageAsync(FormattedMessage message)
    {
        await _message.Channel.SendMessageAsync(embed: message.Convert());
    }


    public async Task<string?> GetAvatarImageUrl(ulong userId)
    {
        var user = _message.Channel.AsGuildChannel()?.Guild.GetUser(userId);
        return user?.GetDisplayAvatarUrl();
    }

    public async Task<List<User>> GetUserMentionsAsync()
    {
        return _message.MentionedUsers.Select(x => Convert((IGuildUser)x)).ToList();
    }

    public async Task<User?> GetUserAsync(ulong userId)
    {
        var user = _message.Channel.AsGuildChannel()?.Guild.GetUser(userId);
        return user == null ? null : Convert(user);
    }

    public async Task<ulong> GetAuthorId()
    {
        return _message.Author.Id;
    }

    //TODO: Can probably make these extension functions or something
    private static User Convert(IGuildUser user)
    {
        return new User
        {
            Id = user.Id,
            EffectiveAvatarUrl = user.GetDisplayAvatarUrl(size: 512),
            Nickname = user.Nickname,
            Username = $"{user.Username}#{user.Discriminator}"
        };
    }

    private static MessageAttachment Convert(Attachment attachment)
    {
        return new MessageAttachment
        {
            Filename = attachment.Filename,
            Url = attachment.Url
        };
    }

    public IGuild? GetGuild()
    {
        return _message.Channel.AsGuildChannel()?.Guild;
    }

    public IVoiceState? GetUserVoiceState()
    {
        if (_message.Author is IVoiceState vs) return vs;
        return null;
    }

    public IMessageChannel? GetChannel()
    {
        return _message.Channel;
    }
}