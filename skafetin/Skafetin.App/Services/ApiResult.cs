namespace Skafetin.App.Services;

public sealed class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResult<T> Ok(T? data) => new() { IsSuccess = true, Data = data };
    public static ApiResult<T> Fail(string message) => new() { IsSuccess = false, ErrorMessage = message };
}

