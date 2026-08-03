using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using FluentValidation;

namespace Auth.Application.Features.Authentication.Queries.GetUserById;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
