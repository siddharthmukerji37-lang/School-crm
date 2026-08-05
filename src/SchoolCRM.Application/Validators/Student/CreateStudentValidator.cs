using FluentValidation;
using SchoolCRM.Application.DTOs.Student;

namespace SchoolCRM.Application.Validators.Student;

public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("Section is required");

        RuleFor(x => x.ClassRoomId)
            .NotEmpty().WithMessage("Class is required");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Today).WithMessage("Date of birth must be a valid date in the past");

        RuleFor(x => x.AdmissionDate)
            .NotEmpty().WithMessage("Admission date is required");
    }
}
