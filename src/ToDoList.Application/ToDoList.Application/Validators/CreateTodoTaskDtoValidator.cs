using FluentValidation;
using ToDoList.Application.DTOs;

namespace ToDoList.Application.Validators;

public class CreateTodoTaskDtoValidator : AbstractValidator<CreateTodoTaskDto>
{
    public CreateTodoTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Due date cannot be in the past")
            .When(x => x.DueDate.HasValue);
    }
}
