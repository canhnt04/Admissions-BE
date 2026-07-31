namespace Shared.Common;

public class Error
{
    public int Code { get; }
    public string Key { get; }
    public string Message { get; }

    public Error(int code, string key, string message)
    {
        Code = code;
        Key = key;
        Message = message;
    }

    public static readonly Error None = new(0, string.Empty, string.Empty);
}
