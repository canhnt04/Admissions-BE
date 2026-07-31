using FluentValidation;
using MediatR;
using Shared.Common.Exceptions;

namespace Shared.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var validationErrors = failures.Select(f => new Shared.Common.Responses.ValidationError
                {
                    Field = f.PropertyName,
                    Message = f.ErrorMessage
                }).ToList();

                var error = new Shared.Common.Error(400, "Validation.Failed", "One or more validation failures occurred.");
                
                throw new Shared.Common.Exceptions.ValidationException(error, validationErrors);
            }
        }

        return await next();
    }
}
