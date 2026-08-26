using FluentValidation;

namespace TaskManagement.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandValidator
    : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title khali nahi ho sakta!")
            .MaximumLength(200).WithMessage("Title 200 characters se zyada nahi ho sakta!");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description 1000 characters se zyada nahi ho sakta!");
    }
}