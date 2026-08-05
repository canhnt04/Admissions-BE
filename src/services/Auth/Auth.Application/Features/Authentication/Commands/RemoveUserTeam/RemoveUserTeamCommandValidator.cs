using FluentValidation;

namespace Auth.Application.Features.Authentication.Commands.RemoveUserTeam;

public class RemoveUserTeamCommandValidator : AbstractValidator<RemoveUserTeamCommand>
{
    public RemoveUserTeamCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
