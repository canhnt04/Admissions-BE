using Shared.Common.Responses;

namespace Shared.Common.Exceptions;

public abstract class CustomException : Exception
{
    public Error Error { get; }

    protected CustomException(Error error) : base(error.Message)
    {
        Error = error;
    }
}

public class ValidationException : CustomException
{
    public IEnumerable<ValidationError>? ValidationErrors { get; }

    public ValidationException(Error error, IEnumerable<ValidationError>? validationErrors = null) : base(error)
    {
        ValidationErrors = validationErrors;
    }
}

public class UnauthorizedException : CustomException
{
    public UnauthorizedException(Error error) : base(error) { }
}

public class ForbiddenException : CustomException
{
    public ForbiddenException(Error error) : base(error) { }
}

public class NotFoundException : CustomException
{
    public NotFoundException(Error error) : base(error) { }
}

public class ConflictException : CustomException
{
    public ConflictException(Error error) : base(error) { }
}

public class BusinessException : CustomException
{
    public BusinessException(Error error) : base(error) { }
}
