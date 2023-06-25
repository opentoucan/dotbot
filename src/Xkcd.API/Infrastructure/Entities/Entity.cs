namespace Xkcd.API.Infrastructure.Entities;

public abstract class Entity
{
    Guid _Id;
    public virtual Guid Id
    {
        get => _Id;
        set => _Id = value;
    }
}