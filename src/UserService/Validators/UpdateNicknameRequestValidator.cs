using FluentValidation;
using KnowledgeControl.UserService.Contracts.Users;

namespace KnowledgeControl.UserService.Validators;

public class UpdateNicknameRequestValidator : AbstractValidator<UpdateNicknameRequest>
{
    public UpdateNicknameRequestValidator()
    {
        RuleFor(x => x.NewNickname)
            .NotEmpty().WithMessage("Новый никнейм обязателен.")
            .MinimumLength(3).WithMessage("Никнейм должен содержать минимум 3 символа.")
            .MaximumLength(64).WithMessage("Никнейм не должен превышать 64 символа.");
    }
}