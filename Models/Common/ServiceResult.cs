namespace LinguaLearn.Mobile.Models.Common;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Exception? Exception { get; private set; }

    private ServiceResult(bool isSuccess, T? data, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>(true, data, null, null);
    }

    public static ServiceResult<T> Failure(string errorMessage)
    {
        return new ServiceResult<T>(false, default(T), errorMessage, null);
    }

    public static ServiceResult<T> Failure(string errorMessage, Exception exception)
    {
        return new ServiceResult<T>(false, default(T), errorMessage, exception);
    }

    public static ServiceResult<T> Failure(Exception exception)
    {
        return new ServiceResult<T>(false, default(T), exception.Message, exception);
    }
}

public static class ServiceResult
{
    public static ServiceResult<T> Success<T>(T data)
    {
        return ServiceResult<T>.Success(data);
    }

    public static ServiceResult<T> Failure<T>(string errorMessage)
    {
        return ServiceResult<T>.Failure(errorMessage);
    }

    public static ServiceResult<T> Failure<T>(string errorMessage, Exception exception)
    {
        return ServiceResult<T>.Failure(errorMessage, exception);
    }

    public static ServiceResult<T> Failure<T>(Exception exception)
    {
        return ServiceResult<T>.Failure(exception);
    }
}