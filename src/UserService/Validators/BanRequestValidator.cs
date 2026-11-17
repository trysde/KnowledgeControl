using FluentValidation;
using UserService.Contracts.Bans;

namespace KnowledgeControl.UserService.Validators;

public class BanRequestValidator : AbstractValidator<BanRequest>
{
    public BanRequestValidator()
    {
        RuleFor(x => x.UserUid)
            .NotEmpty().WithMessage("Не указан идентификатор пользователя.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Причина блокировки обязательна.")
            .MaximumLength(500).WithMessage("Причина блокировки не должна превышать 500 символов.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(_ => DateTimeOffset.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("Дата окончания бана должна быть в будущем.");
    }
}