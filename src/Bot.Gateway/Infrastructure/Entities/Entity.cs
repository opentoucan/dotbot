namespace Bot.Gateway.Infrastructure.Entities;

public abstract class Entity
{
    Guid _id;
    public virtual Guid Id
    {
        get => _id;
        set => _id = value;
    }
}