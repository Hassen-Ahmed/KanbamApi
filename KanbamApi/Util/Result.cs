namespace KanbamApi.Util;

public class Error
{
    public int CodeStatus { get; }
    public string Message { get; }

    public Error(int code, string message)
    {
        CodeStatus = code;
        Message = message;
    }

    public override string ToString() => $"{CodeStatus}: {Message}";
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public T Value { get; }

    protected Result(bool isSuccess, T value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null!);

    public static Result<T> Failure(Error error) => new Result<T>(false, default!, error);
}
