using FluentValidation;
using UserService.Contracts.Users;

namespace KnowledgeControl.UserService.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Текущий пароль обязателен.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Новый пароль обязателен.")
            .MinimumLength(8).WithMessage("Новый пароль должен содержать минимум 8 символов.")
            .MaximumLength(100).WithMessage("Новый пароль не должен превышать 100 символов.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Новый пароль не должен совпадать со старым.");
    }
}