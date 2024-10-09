namespace Dotbot.Gateway.Application.InteractionCommands.Exceptions;

public class CommandValidationException(string message) : Exception(message);