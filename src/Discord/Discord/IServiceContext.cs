using Discord.Application.Models;

namespace Discord.Discord;

public interface IServiceContext
{
    Privilege GetPrivilege();
    Task ReplyAsync(string msg);
    Task ReplyAsync(MessageComponent msg);
    Task ReplyFileAsync(string fileName, Stream fs);
    Task SendMessageAsync(string msg);
    Task SendFileAsync(string fileName, Stream fs);
    Task<string> GetServerId();
    Task<string> GetChannelId();
    Task<bool> HasAttachments();
    Task<IReadOnlyCollection<MessageAttachment>> GetAttachments();
    Task SendFormattedMessageAsync(FormattedMessage message); 
    Task<List<User>> GetUserMentionsAsync();
    Task<User?> GetUserAsync(ulong userId);
    Task<ulong> GetAuthorId();
}