namespace Discord.Exceptions;

public class BotCommandPermissionException : Exception
{
    public BotCommandPermissionException()
    { }

    public BotCommandPermissionException(string message)
        : base(message)
    { }

    public BotCommandPermissionException(string message, Exception innerException)
        : base(message, innerException)
    { }
}