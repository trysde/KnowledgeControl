using FluentValidation;
using TestCreationService.Contracts.Tests;

namespace TestCreationService.Validators;

public class UpdateTestRequestValidator : AbstractValidator<UpdateTestRequest>
{
    public UpdateTestRequestValidator()
    {
        RuleFor(x => x.CategoryUid)
            .NotEmpty().WithMessage("Не указан идентификатор категории.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название теста обязательно.")
            .MinimumLength(3).WithMessage("Название теста должно содержать минимум 3 символа.")
            .MaximumLength(200).WithMessage("Название теста не должно превышать 200 символов.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание теста не должно превышать 2000 символов.")
            .When(x => x.Description is not null);

        RuleFor(x => x.TimeLimitSeconds)
            .GreaterThan(0).WithMessage("Ограничение по времени должно быть больше 0 секунд.")
            .When(x => x.TimeLimitSeconds.HasValue);

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0).WithMessage("Максимальное количество попыток должно быть больше 0.")
            .When(x => x.MaxAttempts.HasValue);

        RuleFor(x => x.PassingScore)
            .InclusiveBetween(0, 100).WithMessage("Проходной балл должен быть в диапазоне от 0 до 100.");

        RuleFor(x => x.StartsAt)
            .LessThan(x => x.EndsAt)
            .When(x => x.StartsAt.HasValue && x.EndsAt.HasValue)
            .WithMessage("Дата начала теста должна быть раньше даты окончания.");

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Тест должен содержать хотя бы один вопрос.")
            .Must(q => q.Select(i => i.QuestionUid).Distinct().Count() == q.Count)
            .WithMessage("Вопросы в тесте не должны дублироваться.");

        RuleForEach(x => x.Questions).ChildRules(q =>
        {
            q.RuleFor(i => i.QuestionUid)
                .NotEmpty().WithMessage("Не указан идентификатор вопроса.");

            q.RuleFor(i => i.OrderIndex)
                .GreaterThanOrEqualTo(0).WithMessage("Порядковый номер вопроса не может быть отрицательным.");

            q.RuleFor(i => i.Points)
                .GreaterThan(0).WithMessage("Количество баллов за вопрос должно быть больше 0.");
        });
    }
}