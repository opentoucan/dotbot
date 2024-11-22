using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.SeedWork;

namespace Dotbot.Infrastructure.Repositories;

public interface IGuildRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<Guild?> GetByExternalIdAsync(string externalId);
    Guild Add(Guild guild);
    void Update(Guild guild);
    void Remove(Guild guild);
}