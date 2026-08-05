using FluentValidation;
using SchoolCRM.Application.DTOs.School;

namespace SchoolCRM.Application.Validators.School;

public class CreateSchoolValidator : AbstractValidator<CreateSchoolDto>
{
    public CreateSchoolValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required")
            .MaximumLength(200).WithMessage("School name must not exceed 200 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("School code is required")
            .MaximumLength(50).WithMessage("School code must not exceed 50 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number");
    }
}
