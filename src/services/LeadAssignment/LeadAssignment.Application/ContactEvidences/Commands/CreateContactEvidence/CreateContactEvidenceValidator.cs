using FluentValidation;

namespace LeadAssignment.Application.ContactEvidences.Commands.CreateContactEvidence
{
    public class CreateContactEvidenceValidator : AbstractValidator<CreateContactEvidenceCommand>
    {
        public CreateContactEvidenceValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Thiếu ID khách hàng");

            RuleFor(x => x.ConsultantId)
                .NotEmpty().WithMessage("Thiếu ID nhân viên tư vấn");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Loại bằng chứng không hợp lệ");

            // Nếu là ghi âm, bắt buộc có FileUrl
            RuleFor(x => x.FileUrl)
                .NotEmpty()
                .When(x => x.Type == Domain.Entities.ContactEvidenceType.CallRecording)
                .WithMessage("Bằng chứng ghi âm phải có URL file");

            // Nếu là ghi chú, bắt buộc có mô tả ≥ 20 ký tự
            RuleFor(x => x.Description)
                .NotEmpty()
                .When(x => x.Type == Domain.Entities.ContactEvidenceType.Note)
                .WithMessage("Ghi chú tư vấn không được để trống");

            RuleFor(x => x.Description)
                .MinimumLength(20)
                .When(x => x.Type == Domain.Entities.ContactEvidenceType.Note && !string.IsNullOrEmpty(x.Description))
                .WithMessage("Ghi chú tư vấn phải có ít nhất 20 ký tự");

            // Nếu là thay đổi status, bắt buộc có OldStatusValue + NewStatusValue
            RuleFor(x => x.NewStatusValue)
                .NotEmpty()
                .When(x => x.Type == Domain.Entities.ContactEvidenceType.StatusChange)
                .WithMessage("Phải có giá trị trạng thái mới khi loại bằng chứng là Thay đổi trạng thái");
        }
    }
}
