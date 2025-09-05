using System.Net;

namespace Dotbot.Api.Services;

public class ServiceResult<T> where T : class
{
    public ServiceResult(T? value)
    {
        Value = value;
        IsSuccess = true;
    }

    public ServiceResult(HttpStatusCode statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
        IsSuccess = false;
    }

    public ServiceResult(Exception exception, string message)
    {
        Exception = exception;
        Message = message;
        IsSuccess = false;
    }

    public T? Value { get; init; }
    public bool IsSuccess { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public Exception? Exception { get; init; }
    public string? Message { get; init; }

    public static ServiceResult<T> Success()
    {
        return new ServiceResult<T>(null);
    }

    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(value);
    }

    public static ServiceResult<T> Error(HttpStatusCode statusCode, string message)
    {
        return new ServiceResult<T>(statusCode, message);
    }

    public static ServiceResult<T> Error(Exception exception, string message)
    {
        return new ServiceResult<T>(exception, message);
    }
}