using FluentValidation;
using UserService.Contracts.Auth;

namespace KnowledgeControl.UserService.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Никнейм обязателен.")
            .MinimumLength(3).WithMessage("Никнейм должен содержать минимум 3 символа.")
            .MaximumLength(64).WithMessage("Никнейм не должен превышать 64 символа.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат Email.")
            .MaximumLength(255).WithMessage("Email не должен превышать 255 символов.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов.")
            .MaximumLength(100).WithMessage("Пароль не должен превышать 100 символов.");
    }
}