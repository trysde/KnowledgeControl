using FluentValidation;
using SearchService.Contracts.Search;

namespace SearchService.Validators;

public class SearchTestsRequestValidator : AbstractValidator<SearchTestsRequest>
{
    private static readonly string[] AllowedSortValues =
        ["relevance", "popular", "newest", "pass_rate", "avg_score"];

    public SearchTestsRequestValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(200).WithMessage("Поисковый запрос не должен превышать 200 символов.")
            .When(x => x.Q is not null);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Название категории не должно превышать 100 символов.")
            .When(x => x.Category is not null);

        RuleFor(x => x.Tag)
            .MaximumLength(50).WithMessage("Название тега не должно превышать 50 символов.")
            .When(x => x.Tag is not null);

        RuleFor(x => x.Sort)
            .NotEmpty().WithMessage("Параметр сортировки не может быть пустым.")
            .Must(s => AllowedSortValues.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Допустимые значения сортировки: {string.Join(", ", AllowedSortValues)}.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Номер страницы должен быть больше или равен 1.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100).WithMessage("Размер страницы должен быть в диапазоне от 1 до 100.");
    }
}