using FluentValidation;
using TestTakingService.Contracts.Attempts;

namespace TestTakingService.Validators;

public class SubmitAnswerRequestValidator : AbstractValidator<SubmitAnswerRequest>
{
    public SubmitAnswerRequestValidator()
    {
        RuleFor(x => x.QuestionUid)
            .NotEmpty().WithMessage("Не указан идентификатор вопроса.");

        RuleFor(x => x.SelectedOptionUids)
            .Must(uids => uids.All(id => id != Guid.Empty))
            .WithMessage("Список выбранных вариантов содержит пустой идентификатор.")
            .Must(uids => uids.Distinct().Count() == uids.Count)
            .WithMessage("Выбранные варианты ответа не должны дублироваться.");

        RuleFor(x => x.TextAnswer)
            .MaximumLength(500).WithMessage("Текстовый ответ не должен превышать 500 символов.")
            .When(x => x.TextAnswer is not null);

        RuleFor(x => x)
            .Must(x => x.SelectedOptionUids.Count > 0 || !string.IsNullOrWhiteSpace(x.TextAnswer))
            .WithMessage("Необходимо выбрать хотя бы один вариант ответа или ввести текстовый ответ.")
            .Must(x => !(x.SelectedOptionUids.Count > 0 && !string.IsNullOrWhiteSpace(x.TextAnswer)))
            .WithMessage("Нельзя одновременно передавать выбранные варианты и текстовый ответ.");
    }
}