using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;
using Shared.Contracts.Events;
using TestCreationService.Contracts.Tests;
using TestCreationService.Data;
using TestCreationService.Domain.Entities;
using TestCreationService.Domain.Enums;
using TestCreationService.Services.Interfaces;

namespace TestCreationService.Services.Implementations;

public class TestService(
    TestCreationDbContext dbContext,
    IMessagePublisher messagePublisher) : ITestService
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<TestResponse> CreateAsync(
        Guid authorUid,
        CreateTestRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Uid == request.CategoryUid, cancellationToken)
            ?? throw new NotFoundException($"Категория с Uid {request.CategoryUid} не найдена.");

        var questionsByUid = await LoadAndValidateQuestionsAsync(authorUid, request.Questions, cancellationToken);
        var now = DateTime.UtcNow;

        var test = new Test
        {
            Uid = Guid.NewGuid(),
            AuthorUid = authorUid,
            CategoryId = category.Id,
            Category = category,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            TimeLimitSeconds = request.TimeLimitSeconds,
            MaxAttempts = request.MaxAttempts,
            PassingScore = request.PassingScore,
            StartsAt = request.StartsAt?.UtcDateTime,
            EndsAt = request.EndsAt?.UtcDateTime,
            IsPublic = request.IsPublic,
            IsHidden = false,
            Status = TestStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var item in request.Questions.OrderBy(q => q.OrderIndex))
        {
            var question = questionsByUid[item.QuestionUid];
            var snapshotJson = BuildQuestionSnapshotJson(question, item.OrderIndex, item.Points);

            test.TestQuestions.Add(new TestQuestion
            {
                Test = test,
                QuestionId = question.Id,
                Question = question,
                OrderIndex = item.OrderIndex,
                Points = item.Points,
                Snapshot = snapshotJson
            });
        }

        dbContext.Tests.Add(test);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(test);
    }

    public async Task<TestResponse> UpdateAsync(
        Guid testUid,
        Guid authorUid,
        UpdateTestRequest request,
        CancellationToken cancellationToken = default)
    {
        var test = await dbContext.Tests
            .Include(t => t.Category)
            .Include(t => t.TestQuestions)
            .FirstOrDefaultAsync(t => t.Uid == testUid, cancellationToken)
            ?? throw new NotFoundException($"Тест с Uid {testUid} не найден.");

        if (test.AuthorUid != authorUid)
        {
            throw new ForbiddenException("Редактировать тест может только его автор.");
        }

        if (test.Status == TestStatus.Blocked || test.IsHidden)
        {
            throw new ForbiddenException("Скрытый или заблокированный модератором тест нельзя редактировать.");
        }

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Uid == request.CategoryUid, cancellationToken)
            ?? throw new NotFoundException($"Категория с Uid {request.CategoryUid} не найдена.");

        var questionsByUid = await LoadAndValidateQuestionsAsync(authorUid, request.Questions, cancellationToken);
        var now = DateTime.UtcNow;

        test.CategoryId = category.Id;
        test.Category = category;
        test.Title = request.Title.Trim();
        test.Description = request.Description?.Trim();
        test.TimeLimitSeconds = request.TimeLimitSeconds;
        test.MaxAttempts = request.MaxAttempts;
        test.PassingScore = request.PassingScore;
        test.StartsAt = request.StartsAt?.UtcDateTime;
        test.EndsAt = request.EndsAt?.UtcDateTime;
        test.IsPublic = request.IsPublic;
        test.UpdatedAt = now;

        var existingByQuestionId = test.TestQuestions.ToDictionary(tq => tq.QuestionId);
        var requestedQuestionIds = request.Questions
            .Select(i => questionsByUid[i.QuestionUid].Id)
            .ToHashSet();

        var toRemove = test.TestQuestions
            .Where(tq => !requestedQuestionIds.Contains(tq.QuestionId))
            .ToList();
        dbContext.TestQuestions.RemoveRange(toRemove);

        foreach (var item in request.Questions.OrderBy(q => q.OrderIndex))
        {
            var question = questionsByUid[item.QuestionUid];
            var snapshotJson = BuildQuestionSnapshotJson(question, item.OrderIndex, item.Points);

            if (existingByQuestionId.TryGetValue(question.Id, out var existingTq))
            {
                existingTq.OrderIndex = item.OrderIndex;
                existingTq.Points = item.Points;
                existingTq.Snapshot = snapshotJson;
            }
            else
            {
                test.TestQuestions.Add(new TestQuestion
                {
                    TestId = test.Id,
                    QuestionId = question.Id,
                    Question = question,
                    OrderIndex = item.OrderIndex,
                    Points = item.Points,
                    Snapshot = snapshotJson
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (test.Status == TestStatus.Published)
        {
            var tags = ExtractDistinctTags(questionsByUid.Values);
            await messagePublisher.PublishAsync(new TestUpdatedEvent
            {
                TestUid = test.Uid,
                Title = test.Title,
                Description = test.Description,
                AuthorUid = test.AuthorUid,
                Category = test.Category.Name,
                Tags = tags,
                OccurredAt = new DateTimeOffset(now, TimeSpan.Zero)
            }, cancellationToken);
        }

        return MapToResponse(test);
    }

    public async Task<PublishTestResponse> PublishAsync(
        Guid testUid,
        Guid authorUid,
        CancellationToken cancellationToken = default)
    {
        var test = await dbContext.Tests
            .Include(t => t.Category)
            .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                    .ThenInclude(q => q.QuestionTags)
                        .ThenInclude(qt => qt.Tag)
            .FirstOrDefaultAsync(t => t.Uid == testUid, cancellationToken)
            ?? throw new NotFoundException($"Тест с Uid {testUid} не найден.");

        if (test.AuthorUid != authorUid)
        {
            throw new ForbiddenException("Опубликовать тест может только его автор.");
        }

        if (test.Status == TestStatus.Blocked || test.IsHidden)
        {
            throw new ForbiddenException("Заблокированный или скрытый тест нельзя опубликовать.");
        }

        if (test.Status == TestStatus.Published)
        {
            throw new ConflictException("Тест уже опубликован.");
        }

        if (test.TestQuestions.Count == 0)
        {
            throw new BadRequestException("Нельзя опубликовать тест без вопросов.");
        }

        var now = DateTime.UtcNow;
        test.Status = TestStatus.Published;
        test.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        var publishedAt = new DateTimeOffset(now, TimeSpan.Zero);
        var tags = ExtractDistinctTags(test.TestQuestions.Select(tq => tq.Question));

        await messagePublisher.PublishAsync(new TestPublishedEvent
        {
            TestUid = test.Uid,
            Title = test.Title,
            Description = test.Description,
            AuthorUid = test.AuthorUid,
            Category = test.Category.Name,
            Tags = tags,
            PublishedAt = publishedAt
        }, cancellationToken);

        return new PublishTestResponse(test.Uid, test.Status.ToString(), publishedAt);
    }

    public async Task UnpublishAsync(
        Guid testUid,
        Guid authorUid,
        CancellationToken cancellationToken = default)
    {
        var test = await dbContext.Tests
            .Include(t => t.Category)
            .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                    .ThenInclude(q => q.QuestionTags)
                        .ThenInclude(qt => qt.Tag)
            .FirstOrDefaultAsync(t => t.Uid == testUid, cancellationToken)
            ?? throw new NotFoundException($"Тест с Uid {testUid} не найден.");

        if (test.AuthorUid != authorUid)
        {
            throw new ForbiddenException("Снять тест с публикации может только его автор.");
        }

        if (test.Status != TestStatus.Published)
        {
            throw new BadRequestException("Снять с публикации можно только опубликованный тест.");
        }

        var now = DateTime.UtcNow;
        test.Status = TestStatus.Draft;
        test.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        var tags = ExtractDistinctTags(test.TestQuestions.Select(tq => tq.Question));

        await messagePublisher.PublishAsync(new TestUnpublishedEvent
        {
            TestUid = test.Uid,
            Title = test.Title,
            Description = test.Description,
            AuthorUid = test.AuthorUid,
            Category = test.Category.Name,
            Tags = tags,
            OccurredAt = new DateTimeOffset(now, TimeSpan.Zero)
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<TestResponse>> GetMyAsync(
        Guid authorUid,
        CancellationToken cancellationToken = default)
    {
        var tests = await dbContext.Tests
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.TestQuestions)
            .Where(t => t.AuthorUid == authorUid)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tests.Select(MapToResponse).ToList();
    }

    public async Task<TestDetailResponse> GetByIdAsync(
        Guid testUid,
        Guid authorUid,
        CancellationToken cancellationToken = default)
    {
        var test = await dbContext.Tests
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
            .FirstOrDefaultAsync(t => t.Uid == testUid, cancellationToken)
            ?? throw new NotFoundException($"Тест с Uid {testUid} не найден.");

        if (test.AuthorUid != authorUid)
        {
            throw new ForbiddenException("Просматривать детали теста для редактирования может только его автор.");
        }

        var questions = test.TestQuestions
            .OrderBy(tq => tq.OrderIndex)
            .Select(tq => new TestQuestionItemResponse(
                QuestionUid: tq.Question.Uid,
                OrderIndex: tq.OrderIndex,
                Points: tq.Points))
            .ToList();

        return new TestDetailResponse(
            Uid: test.Uid,
            CategoryUid: test.Category.Uid,
            Title: test.Title,
            Description: test.Description,
            TimeLimitSeconds: test.TimeLimitSeconds,
            MaxAttempts: test.MaxAttempts,
            PassingScore: test.PassingScore,
            IsPublic: test.IsPublic,
            Status: test.Status.ToString(),
            Questions: questions);
    }

    public async Task<TestSnapshotResponse> GetSnapshotAsync(
            Guid testUid,
            bool includeCorrectAnswers = true,
            CancellationToken cancellationToken = default)
    {
        var test = await dbContext.Tests
            .AsNoTracking()
            .Include(t => t.TestQuestions)
            .FirstOrDefaultAsync(t => t.Uid == testUid, cancellationToken)
            ?? throw new NotFoundException($"Тест с Uid {testUid} не найден.");

        if (test.IsHidden || test.Status == TestStatus.Blocked)
        {
            throw new ForbiddenException("Тест недоступен.");
        }

        if (!includeCorrectAnswers && (test.Status != TestStatus.Published || !test.IsPublic))
        {
            throw new ForbiddenException("Публичный снапшот доступен только для опубликованных публичных тестов.");
        }

        var questionSnapshots = new List<QuestionSnapshotResponse>();

        foreach (var tq in test.TestQuestions.OrderBy(q => q.OrderIndex))
        {
            var deserialized = JsonSerializer.Deserialize<QuestionSnapshotResponse>(tq.Snapshot, SnapshotJsonOptions)
                ?? throw new InvalidOperationException($"Не удалось десериализовать снапшот вопроса {tq.QuestionId}.");

            if (!includeCorrectAnswers)
            {
                deserialized = deserialized with
                {
                    CorrectTextAnswer = null,
                    Options = deserialized.Options
                        .Select(o => o with { IsCorrect = null })
                        .ToList()
                };
            }

            questionSnapshots.Add(deserialized);
        }

        return new TestSnapshotResponse(
            TestUid: test.Uid,
            AuthorUid: test.AuthorUid,
            Title: test.Title,
            Description: test.Description,
            TimeLimitSeconds: test.TimeLimitSeconds,
            MaxAttempts: test.MaxAttempts,
            PassingScore: test.PassingScore,
            StartsAt: test.StartsAt.HasValue ? new DateTimeOffset(test.StartsAt.Value, TimeSpan.Zero) : null,
            EndsAt: test.EndsAt.HasValue ? new DateTimeOffset(test.EndsAt.Value, TimeSpan.Zero) : null,
            Status: test.Status.ToString(),
            Questions: questionSnapshots);
    }
    private async Task<Dictionary<Guid, Question>> LoadAndValidateQuestionsAsync(
        Guid authorUid,
        IReadOnlyList<TestQuestionItemRequest> items,
        CancellationToken cancellationToken)
    {
        var requestedUids = items.Select(i => i.QuestionUid).Distinct().ToList();

        var questions = await dbContext.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .Where(q => requestedUids.Contains(q.Uid) && !q.IsDeleted && !q.IsBlocked)
            .ToListAsync(cancellationToken);

        var accessibleQuestions = questions
            .Where(q => q.IsPublic || q.AuthorUid == authorUid)
            .ToDictionary(q => q.Uid);

        var missingUids = requestedUids.Where(uid => !accessibleQuestions.ContainsKey(uid)).ToList();
        if (missingUids.Count > 0)
        {
            throw new NotFoundException(
                $"Следующие вопросы не найдены или недоступны: {string.Join(", ", missingUids)}.");
        }

        return accessibleQuestions;
    }

    private static string BuildQuestionSnapshotJson(Question question, int orderIndex, int points)
    {
        var options = question.AnswerOptions
            .OrderBy(o => o.Position)
            .Select(o => new AnswerOptionSnapshotResponse(o.Uid, o.Text, o.IsCorrect, o.Position))
            .ToList();

        var snapshot = new QuestionSnapshotResponse(
            QuestionUid: question.Uid,
            Title: question.Title,
            Text: question.Text,
            QuestionType: question.QuestionType.ToString(),
            OrderIndex: orderIndex,
            Points: points,
            CorrectTextAnswer: question.CorrectTextAnswer,
            Options: options);

        return JsonSerializer.Serialize(snapshot, SnapshotJsonOptions);
    }

    private static IReadOnlyList<string> ExtractDistinctTags(IEnumerable<Question> questions)
    {
        return questions
            .SelectMany(q => q.QuestionTags)
            .Select(qt => qt.Tag.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    private static TestResponse MapToResponse(Test test)
    {
        return new TestResponse(
            Uid: test.Uid,
            AuthorUid: test.AuthorUid,
            CategoryUid: test.Category.Uid,
            CategoryName: test.Category.Name,
            Title: test.Title,
            Description: test.Description,
            TimeLimitSeconds: test.TimeLimitSeconds,
            MaxAttempts: test.MaxAttempts,
            PassingScore: test.PassingScore,
            StartsAt: test.StartsAt.HasValue ? new DateTimeOffset(test.StartsAt.Value, TimeSpan.Zero) : null,
            EndsAt: test.EndsAt.HasValue ? new DateTimeOffset(test.EndsAt.Value, TimeSpan.Zero) : null,
            IsPublic: test.IsPublic,
            IsHidden: test.IsHidden,
            Status: test.Status.ToString(),
            QuestionsCount: test.TestQuestions.Count,
            TotalPoints: test.TestQuestions.Sum(tq => tq.Points),
            CreatedAt: new DateTimeOffset(test.CreatedAt, TimeSpan.Zero),
            UpdatedAt: new DateTimeOffset(test.UpdatedAt, TimeSpan.Zero));
    }
}