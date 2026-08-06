namespace Shared.Common.Responses;

public class ApiErrorResponse
{
    public int ErrorCode { get; set; }
    public string ErrorKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public IEnumerable<ValidationError>? ValidationErrors { get; set; }
    public DateTime Timestamp { get; set; } = Shared.Common.Helpers.TimeHelper.VietnamNow;
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
