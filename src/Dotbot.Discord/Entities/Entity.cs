namespace Dotbot.Discord.Entities;

public abstract class Entity
{
    Guid _id;
    public Guid Id
    {
        get => _id;
        set => _id = value;
    }
}