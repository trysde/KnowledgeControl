using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;
using TestCreationService.Contracts.Questions;
using TestCreationService.Data;
using TestCreationService.Domain.Entities;
using TestCreationService.Domain.Enums;
using TestCreationService.Services.Interfaces;

namespace TestCreationService.Services.Implementations;

public class QuestionService(TestCreationDbContext dbContext) : IQuestionService
{
    public async Task<QuestionResponse> CreateAsync(
        Guid authorUid,
        CreateQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Uid == request.CategoryUid, cancellationToken)
            ?? throw new NotFoundException($"Категория с Uid {request.CategoryUid} не найдена.");

        var questionType = Enum.Parse<QuestionType>(request.QuestionType, ignoreCase: true);
        var isPublic = string.Equals(request.Visibility, "Public", StringComparison.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        var question = new Question
        {
            Uid = Guid.NewGuid(),
            AuthorUid = authorUid,
            CategoryId = category.Id,
            Category = category,
            Title = request.Title.Trim(),
            Text = request.Text.Trim(),
            QuestionType = questionType,
            CorrectTextAnswer = questionType == QuestionType.TextInput
                ? request.CorrectTextAnswer?.Trim()
                : null,
            IsPublic = isPublic,
            Version = 1,
            IsDeleted = false,
            IsBlocked = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (questionType is QuestionType.SingleChoice or QuestionType.MultipleChoice)
        {
            foreach (var opt in request.AnswerOptions.OrderBy(o => o.Position))
            {
                question.AnswerOptions.Add(new AnswerOption
                {
                    Uid = Guid.NewGuid(),
                    Text = opt.Text.Trim(),
                    IsCorrect = opt.IsCorrect,
                    Position = opt.Position
                });
            }
        }

        await AttachTagsAsync(question, request.Tags, cancellationToken);

        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(question);
    }

    public async Task<IReadOnlyList<QuestionResponse>> GetMyAsync(
        Guid authorUid,
        CancellationToken cancellationToken = default)
    {
        var questions = await dbContext.Questions
            .AsNoTracking()
            .Include(q => q.Category)
            .Include(q => q.AnswerOptions)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .Where(q => q.AuthorUid == authorUid && !q.IsDeleted)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return questions.Select(MapToResponse).ToList();
    }

    public async Task<QuestionResponse> GetByIdAsync(
        Guid questionUid,
        Guid requesterUid,
        CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions
            .AsNoTracking()
            .Include(q => q.Category)
            .Include(q => q.AnswerOptions)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .FirstOrDefaultAsync(q => q.Uid == questionUid && !q.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Вопрос с Uid {questionUid} не найден.");

        var isAuthor = question.AuthorUid == requesterUid;
        if (!isAuthor && (!question.IsPublic || question.IsBlocked))
        {
            throw new ForbiddenException("Нет доступа к данному вопросу.");
        }

        return MapToResponse(question);
    }

    public async Task<QuestionResponse> UpdateAsync(
        Guid questionUid,
        Guid authorUid,
        UpdateQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions
            .Include(q => q.Category)
            .Include(q => q.AnswerOptions)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .FirstOrDefaultAsync(q => q.Uid == questionUid && !q.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Вопрос с Uid {questionUid} не найден.");

        if (question.AuthorUid != authorUid)
        {
            throw new ForbiddenException("Редактировать вопрос может только его автор.");
        }

        if (question.IsBlocked)
        {
            throw new ForbiddenException("Заблокированный модератором вопрос нельзя редактировать.");
        }

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Uid == request.CategoryUid, cancellationToken)
            ?? throw new NotFoundException($"Категория с Uid {request.CategoryUid} не найдена.");

        var questionType = Enum.Parse<QuestionType>(request.QuestionType, ignoreCase: true);

        question.CategoryId = category.Id;
        question.Category = category;
        question.Title = request.Title.Trim();
        question.Text = request.Text.Trim();
        question.QuestionType = questionType;
        question.CorrectTextAnswer = questionType == QuestionType.TextInput
            ? request.CorrectTextAnswer?.Trim()
            : null;
        question.IsPublic = string.Equals(request.Visibility, "Public", StringComparison.OrdinalIgnoreCase);
        question.Version += 1;
        question.UpdatedAt = DateTime.UtcNow;

        dbContext.AnswerOptions.RemoveRange(question.AnswerOptions);
        question.AnswerOptions.Clear();

        if (questionType is QuestionType.SingleChoice or QuestionType.MultipleChoice)
        {
            foreach (var opt in request.AnswerOptions.OrderBy(o => o.Position))
            {
                question.AnswerOptions.Add(new AnswerOption
                {
                    Uid = Guid.NewGuid(),
                    Text = opt.Text.Trim(),
                    IsCorrect = opt.IsCorrect,
                    Position = opt.Position
                });
            }
        }

        var normalizedNames = request.Tags
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToHashSet();

        var toRemove = question.QuestionTags
            .Where(qt => !normalizedNames.Contains(qt.Tag.Name.ToLowerInvariant()))
            .ToList();
        foreach (var qt in toRemove)
            question.QuestionTags.Remove(qt);

        var existingNames = question.QuestionTags
            .Select(qt => qt.Tag.Name.ToLowerInvariant())
            .ToHashSet();
        var namesToAdd = normalizedNames.Where(n => !existingNames.Contains(n)).ToList();
        if (namesToAdd.Count > 0)
            await AttachTagsAsync(question, namesToAdd, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(question);
    }

    public async Task DeleteAsync(
        Guid questionUid,
        Guid authorUid,
        CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions
            .FirstOrDefaultAsync(q => q.Uid == questionUid && !q.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Вопрос с Uid {questionUid} не найден.");

        if (question.AuthorUid != authorUid)
        {
            throw new ForbiddenException("Удалить вопрос может только его автор.");
        }

        question.IsDeleted = true;
        question.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AttachTagsAsync(
        Question question,
        IReadOnlyList<string> rawTags,
        CancellationToken cancellationToken)
    {
        var normalizedNames = rawTags
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToList();

        if (normalizedNames.Count == 0)
            return;

        var existingTags = await dbContext.Tags
            .Where(t => normalizedNames.Contains(t.Name.ToLower()))
            .ToListAsync(cancellationToken);

        var existingByName = existingTags.ToDictionary(t => t.Name.ToLowerInvariant());

        foreach (var tagName in normalizedNames)
        {
            if (!existingByName.TryGetValue(tagName, out var tag))
            {
                tag = new Tag
                {
                    Uid = Guid.NewGuid(),
                    Name = tagName
                };
                dbContext.Tags.Add(tag);
            }

            question.QuestionTags.Add(new QuestionTag
            {
                Question = question,
                Tag = tag
            });
        }
    }

    private static QuestionResponse MapToResponse(Question question)
    {
        var tags = question.QuestionTags
            .Select(qt => qt.Tag.Name)
            .OrderBy(name => name)
            .ToList();

        var options = question.AnswerOptions
            .OrderBy(o => o.Position)
            .Select(o => new AnswerOptionResponse(o.Uid, o.Text, o.IsCorrect, o.Position))
            .ToList();

        return new QuestionResponse(
            Uid: question.Uid,
            AuthorUid: question.AuthorUid,
            CategoryUid: question.Category.Uid,
            CategoryName: question.Category.Name,
            Title: question.Title,
            Text: question.Text,
            QuestionType: question.QuestionType.ToString(),
            CorrectTextAnswer: question.CorrectTextAnswer,
            IsPublic: question.IsPublic,
            Version: question.Version,
            IsBlocked: question.IsBlocked,
            Tags: tags,
            AnswerOptions: options,
            CreatedAt: new DateTimeOffset(question.CreatedAt, TimeSpan.Zero),
            UpdatedAt: new DateTimeOffset(question.UpdatedAt, TimeSpan.Zero));
    }
}