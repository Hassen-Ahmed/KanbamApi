namespace KanbamApi.Util;

public class Error(int code, string message)
{
    public int CodeStatus { get; } = code;
    public string Message { get; } = message;

    public override string ToString() => $"{CodeStatus}: {Message}";
}

public class Result<T>(bool isSuccess, T value, Error error)
{
    public bool IsSuccess { get; } = isSuccess;
    public bool IsFailure => !IsSuccess;
    public Error Error { get; } = error;
    public T Value { get; } = value;

    public static Result<T> Success(T value) => new(true, value, null!);

    public static Result<T> Failure(Error error) => new(false, default!, error);
}
