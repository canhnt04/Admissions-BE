namespace Shared.Common;

public class Result
{
    public Error Error { get; }

    protected Result(Error error)
    {
        Error = error;
    }

    public static Result Success() => new(Error.None);
    public static Result Failure(Error error) => new(error);
}

public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(T? data, Error error) : base(error)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(data, Error.None);
    public static new Result<T> Failure(Error error) => new(default, error);
}
