using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using FluentValidation;

namespace Auth.Application.Features.Authentication.Commands.AssignUser;

public class AssignUserCommandValidator : AbstractValidator<AssignUserCommand>
{
    public AssignUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Role)
            .IsInEnum().When(x => x.Role.HasValue).WithMessage("Invalid Role.");
    }
}
