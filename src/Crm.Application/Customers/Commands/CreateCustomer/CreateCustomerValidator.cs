using FluentValidation;

namespace Crm.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên khách hàng không được để trống")
                .MaximumLength(50).WithMessage("Tên khách hàng tối đa 50 ký tự");

            RuleFor(x => x.Mobile)
                .NotEmpty().WithMessage("Số điện thoại không được để trống")
                .MaximumLength(50).WithMessage("Số điện thoại tối đa 50 ký tự")
                .Matches(@"^[0-9\+\-\s]+$").WithMessage("Số điện thoại không hợp lệ");

            RuleFor(x => x.Email)
                .MaximumLength(50).WithMessage("Email tối đa 50 ký tự")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email không hợp lệ");

            RuleFor(x => x.TrainingSystem)
                .IsInEnum().WithMessage("Hệ đào tạo không hợp lệ");

            RuleFor(x => x.CreatedBy)
                .NotEmpty().WithMessage("Thiếu thông tin người tạo");
        }
    }
}
