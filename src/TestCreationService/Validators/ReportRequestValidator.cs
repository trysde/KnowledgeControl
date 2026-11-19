using FluentValidation;
using TestCreationService.Contracts.Moderation;
using TestCreationService.Domain.Enums;

namespace TestCreationService.Validators;

public class ReportRequestValidator : AbstractValidator<ReportRequest>
{
    private static readonly string[] AllowedTargetTypes = Enum.GetNames<ModerationTargetType>();

    public ReportRequestValidator()
    {
        RuleFor(x => x.TargetType)
            .NotEmpty().WithMessage("Тип объекта жалобы обязателен.")
            .Must(t => AllowedTargetTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Тип объекта жалобы должен быть одним из: {string.Join(", ", AllowedTargetTypes)}.");

        RuleFor(x => x.TargetUid)
            .NotEmpty().WithMessage("Не указан идентификатор объекта жалобы.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Причина жалобы обязательна.")
            .MinimumLength(5).WithMessage("Причина жалобы должна содержать минимум 5 символов.")
            .MaximumLength(1000).WithMessage("Причина жалобы не должна превышать 1000 символов.");
    }
}