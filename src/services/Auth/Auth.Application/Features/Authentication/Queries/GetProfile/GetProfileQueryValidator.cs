using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using FluentValidation;

namespace Auth.Application.Features.Authentication.Queries.GetProfile;

public class GetProfileQueryValidator : AbstractValidator<GetProfileQuery>
{
    public GetProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
