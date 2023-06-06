using Dotbot.Database.Entities;
using FluentResults;

namespace Dotbot.Database.Repositories;

public interface IMemeRepository: IRepository<Meme>
{
    public Result<List<Meme>> GetMemes(string serverId);
    public Result<Meme> Get(string serverId, string id);
    public Result<Meme> FindByServerIdAndMessageId(string serverId, string messageId);
    public Result<Meme> Create();
}