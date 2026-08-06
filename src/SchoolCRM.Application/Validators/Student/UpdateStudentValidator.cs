using FluentValidation;
using SchoolCRM.Application.DTOs.Student;

namespace SchoolCRM.Application.Validators.Student;

public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
{
    public UpdateStudentValidator()
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

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");

        RuleFor(x => x.AdmissionDate)
            .NotEmpty().WithMessage("Admission date is required");

        RuleFor(x => x.ParentName)
            .MaximumLength(200).WithMessage("Parent name must not exceed 200 characters");

        RuleFor(x => x.ParentPhone)
            .MaximumLength(50).WithMessage("Parent phone must not exceed 50 characters");

        RuleFor(x => x.ParentEmail)
            .EmailAddress().WithMessage("Invalid parent email address")
            .MaximumLength(200).WithMessage("Parent email must not exceed 200 characters");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(5000).WithMessage("Notes must not exceed 5000 characters");
    }
}
