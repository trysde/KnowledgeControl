using FluentValidation;
using TestCreationService.Contracts.Questions;
using TestCreationService.Domain.Enums;

namespace TestCreationService.Validators;

public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
{
    private static readonly string[] AllowedQuestionTypes = Enum.GetNames<QuestionType>();
    private static readonly string[] AllowedVisibilities = ["Public", "Private"];

    public UpdateQuestionRequestValidator()
    {
        RuleFor(x => x.CategoryUid)
            .NotEmpty().WithMessage("Не указан идентификатор категории.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок вопроса обязателен.")
            .MinimumLength(3).WithMessage("Заголовок вопроса должен содержать минимум 3 символа.")
            .MaximumLength(200).WithMessage("Заголовок вопроса не должен превышать 200 символов.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст вопроса обязателен.")
            .MinimumLength(3).WithMessage("Текст вопроса должен содержать минимум 3 символа.")
            .MaximumLength(2000).WithMessage("Текст вопроса не должен превышать 2000 символов.");

        RuleFor(x => x.QuestionType)
            .NotEmpty().WithMessage("Тип вопроса обязателен.")
            .Must(t => AllowedQuestionTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Тип вопроса должен быть одним из: {string.Join(", ", AllowedQuestionTypes)}.");

        RuleFor(x => x.Visibility)
            .NotEmpty().WithMessage("Видимость вопроса обязательна.")
            .Must(v => AllowedVisibilities.Contains(v, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Видимость должна быть 'Public' или 'Private'.");

        RuleForEach(x => x.AnswerOptions).ChildRules(option =>
        {
            option.RuleFor(o => o.Text)
                .NotEmpty().WithMessage("Текст варианта ответа обязателен.")
                .MaximumLength(500).WithMessage("Текст варианта ответа не должен превышать 500 символов.");

            option.RuleFor(o => o.Position)
                .GreaterThanOrEqualTo(0).WithMessage("Позиция варианта ответа не может быть отрицательной.");
        });

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Тег не может быть пустым.")
            .MaximumLength(50).WithMessage("Длина тега не должна превышать 50 символов.");

        When(x => string.Equals(x.QuestionType, nameof(QuestionType.SingleChoice), StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.AnswerOptions)
                .Must(opts => opts.Count >= 2)
                .WithMessage("Для вопроса с одиночным выбором необходимо минимум 2 варианта ответа.")
                .Must(opts => opts.Count(o => o.IsCorrect) == 1)
                .WithMessage("Для вопроса с одиночным выбором должен быть ровно 1 правильный вариант ответа.");
        });

        When(x => string.Equals(x.QuestionType, nameof(QuestionType.MultipleChoice), StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.AnswerOptions)
                .Must(opts => opts.Count >= 2)
                .WithMessage("Для вопроса с множественным выбором необходимо минимум 2 варианта ответа.")
                .Must(opts => opts.Count(o => o.IsCorrect) >= 1)
                .WithMessage("Для вопроса с множественным выбором должен быть хотя бы 1 правильный вариант ответа.");
        });

        When(x => string.Equals(x.QuestionType, nameof(QuestionType.TextInput), StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.CorrectTextAnswer)
                .NotEmpty().WithMessage("Для текстового вопроса необходимо указать правильный ответ.")
                .MaximumLength(500).WithMessage("Правильный текстовый ответ не должен превышать 500 символов.");
        });
    }
}