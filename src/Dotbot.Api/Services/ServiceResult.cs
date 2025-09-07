namespace Dotbot.Api.Services;

public class ServiceResult<T> where T : class
{
    protected ServiceResult(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    protected ServiceResult(string message)
    {
        IsSuccess = true;
        Message = message;
    }

    protected ServiceResult(ErrorResult errorResult)
    {
        IsSuccess = false;
        ErrorResult = errorResult;
    }

    public T? Value { get; protected set; }
    public bool IsSuccess { get; protected set; }
    public string? Message { get; protected set; }

    public ErrorResult? ErrorResult { get; protected set; }

    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(value);
    }

    public static ServiceResult<T> SuccessWithMessage(string message)
    {
        return new ServiceResult<T>(message);
    }

    public static ServiceResult<T> Error(string message)
    {
        return new ServiceResult<T>(new ErrorResult { ErrorMessage = message });
    }
}

public class ErrorResult
{
    public required string ErrorMessage { get; init; }
}