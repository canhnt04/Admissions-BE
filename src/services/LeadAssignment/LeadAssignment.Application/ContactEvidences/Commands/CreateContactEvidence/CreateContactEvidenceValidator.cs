using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using FluentValidation;

namespace LeadAssignment.Application.ContactEvidences.Commands.CreateContactEvidence
{
    public class CreateContactEvidenceValidator : AbstractValidator<CreateContactEvidenceCommand>
    {
        public CreateContactEvidenceValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId không được để trống");
            RuleFor(x => x.ConsultantId).NotEmpty().WithMessage("ConsultantId không được để trống");
        }
    }
}
