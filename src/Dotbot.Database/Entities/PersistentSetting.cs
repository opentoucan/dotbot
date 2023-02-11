namespace Dotbot.Database.Entities;

public class PersistentSetting: Entity
{
    public string Key { get; set; }
    public object Value { get; set; }

    public T? Get<T>()
    {
        try
        {
            return (T) Value;
        }
        catch(InvalidCastException ex)
        {
            return default;
        }
    }
}