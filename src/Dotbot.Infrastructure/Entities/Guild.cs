using Dotbot.Infrastructure.SeedWork;

namespace Dotbot.Infrastructure.Entities;

public class Guild : ValueObject
{
    public string ExternalId { get; private set; } = null!;
    public bool IsServer { get; private set; }

    protected Guild() { }
    public Guild(string externalId, bool isServer)
    {
        ExternalId = externalId;
        IsServer = isServer;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ExternalId;
        yield return IsServer;
    }
}