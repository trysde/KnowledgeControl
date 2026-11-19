using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;
using Shared.Contracts.Events;
using TestTakingService.Contracts.Attempts;
using TestTakingService.Contracts.Snapshots;
using TestTakingService.Domain.Entities;
using TestTakingService.Domain.Enums;
using TestTakingService.Domain.ValueObjects;
using TestTakingService.Infrastructure.Persistence;
using TestTakingService.Services.Interfaces;

namespace TestTakingService.Services.Implementations;

public class AttemptService(
    TestTakingDbContext dbContext,
    ITestCreationClient testCreationClient,
    IAttemptSnapshotCache snapshotCache,
    GradingService gradingService,
    IMessagePublisher messagePublisher) : IAttemptService
{
    public async Task<StartAttemptResponse> StartAsync(
        Guid testUid,
        Guid userUid,
        CancellationToken cancellationToken = default)
    {
        var remoteSnapshot = await testCreationClient.GetTestSnapshotAsync(testUid, cancellationToken);

        if (!string.Equals(remoteSnapshot.Status, "Published", StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Тест не опубликован и недоступен для прохождения.");
        }

        var nowOffset = DateTimeOffset.UtcNow;
        var nowUtc = nowOffset.UtcDateTime;

        if (remoteSnapshot.StartsAt.HasValue && nowOffset < remoteSnapshot.StartsAt.Value)
        {
            throw new ForbiddenException("Время прохождения теста ещё не началось.");
        }

        if (remoteSnapshot.EndsAt.HasValue && nowOffset > remoteSnapshot.EndsAt.Value)
        {
            throw new ForbiddenException("Время прохождения теста уже истекло.");
        }

        if (remoteSnapshot.MaxAttempts.HasValue)
        {
            var usedAttemptsCount = await dbContext.Attempts
                .CountAsync(a => a.UserUid == userUid && a.TestUid == testUid, cancellationToken);

            if (usedAttemptsCount >= remoteSnapshot.MaxAttempts.Value)
            {
                throw new ConflictException($"Исчерпан лимит попыток ({remoteSnapshot.MaxAttempts.Value}) для данного теста.");
            }
        }

        var internalSnapshot = MapToInternalSnapshot(remoteSnapshot);
        DateTime? expiresAtUtc = remoteSnapshot.TimeLimitSeconds is > 0
            ? nowUtc.AddSeconds(remoteSnapshot.TimeLimitSeconds.Value)
            : null;

        var attempt = new Attempt
        {
            Uid = Guid.NewGuid(),
            TestUid = remoteSnapshot.TestUid,
            UserUid = userUid,
            Status = AttemptStatus.InProgress,
            Snapshot = internalSnapshot,
            SnapshotVersion = 1,
            StartedAt = nowUtc,
            ExpiresAt = expiresAtUtc,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        dbContext.Attempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        await snapshotCache.SetAsync(
            attempt.Uid,
            internalSnapshot,
            remoteSnapshot.TimeLimitSeconds,
            cancellationToken);

        return new StartAttemptResponse(
            AttemptUid: attempt.Uid,
            TestUid: attempt.TestUid,
            Status: attempt.Status.ToString(),
            StartedAt: new DateTimeOffset(attempt.StartedAt, TimeSpan.Zero),
            ExpiresAt: attempt.ExpiresAt.HasValue
                ? new DateTimeOffset(attempt.ExpiresAt.Value, TimeSpan.Zero)
                : null,
            TestSnapshot: MapToClientSnapshot(internalSnapshot));
    }

    public async Task<IReadOnlyList<AttemptResponse>> GetMyAsync(
        Guid userUid,
        CancellationToken cancellationToken = default)
    {
        var attempts = await dbContext.Attempts
            .Include(a => a.Answers)
            .Where(a => a.UserUid == userUid)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(cancellationToken);

        var nowUtc = DateTime.UtcNow;
        var hasExpiredUpdates = false;

        foreach (var attempt in attempts)
        {
            if (TryMarkExpired(attempt, nowUtc))
                hasExpiredUpdates = true;
        }

        if (hasExpiredUpdates)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return attempts.Select(MapToAttemptResponse).ToList();
    }

    public async Task<AttemptResponse> GetByUidAsync(
        Guid attemptUid,
        Guid userUid,
        CancellationToken cancellationToken = default)
    {
        var attempt = await dbContext.Attempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Uid == attemptUid, cancellationToken)
            ?? throw new NotFoundException($"Попытка с Uid {attemptUid} не найдена.");

        if (attempt.UserUid != userUid)
        {
            throw new ForbiddenException("Нет доступа к данной попытке.");
        }

        if (TryMarkExpired(attempt, DateTime.UtcNow))
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await snapshotCache.RemoveAsync(attempt.Uid, cancellationToken);
        }

        return MapToAttemptResponse(attempt);
    }

    public async Task<SavedAnswerDto> SubmitAnswerAsync(
        Guid attemptUid,
        Guid userUid,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var attempt = await dbContext.Attempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Uid == attemptUid, cancellationToken)
            ?? throw new NotFoundException($"Попытка с Uid {attemptUid} не найдена.");

        if (attempt.UserUid != userUid)
        {
            throw new ForbiddenException("Нет доступа к данной попытке.");
        }

        var nowUtc = DateTime.UtcNow;

        if (TryMarkExpired(attempt, nowUtc))
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await snapshotCache.RemoveAsync(attempt.Uid, cancellationToken);
            throw new BadRequestException("Время на прохождение попытки истекло.");
        }

        if (attempt.Status != AttemptStatus.InProgress)
        {
            throw new BadRequestException("Нельзя отправлять ответы в завершённую или просроченную попытку.");
        }

        var snapshot = await snapshotCache.GetAsync(attempt.Uid, cancellationToken) ?? attempt.Snapshot;
        var question = snapshot.Questions.FirstOrDefault(q => q.QuestionUid == request.QuestionUid)
            ?? throw new NotFoundException($"Вопрос с Uid {request.QuestionUid} не найден в данной попытке.");

        ValidateAnswerAgainstQuestion(question, request);

        var selectedOptionUids = request.SelectedOptionUids.Distinct().ToList();
        var trimmedTextAnswer = string.IsNullOrWhiteSpace(request.TextAnswer)
            ? null
            : request.TextAnswer.Trim();

        // Upsert по (attempt_id, question_uid)
        var existingAnswer = attempt.Answers.FirstOrDefault(a => a.QuestionUid == request.QuestionUid);

        if (existingAnswer is null)
        {
            existingAnswer = new Answer
            {
                AttemptId = attempt.Id,
                QuestionUid = request.QuestionUid,
                SelectedOptionUids = selectedOptionUids,
                TextAnswer = trimmedTextAnswer,
                AnsweredAt = nowUtc
            };
            dbContext.Answers.Add(existingAnswer);
        }
        else
        {
            existingAnswer.SelectedOptionUids = selectedOptionUids;
            existingAnswer.TextAnswer = trimmedTextAnswer;
            existingAnswer.AnsweredAt = nowUtc;
        }

        attempt.UpdatedAt = nowUtc;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SavedAnswerDto(
            QuestionUid: existingAnswer.QuestionUid,
            SelectedOptionUids: existingAnswer.SelectedOptionUids,
            TextAnswer: existingAnswer.TextAnswer,
            AnsweredAt: new DateTimeOffset(existingAnswer.AnsweredAt, TimeSpan.Zero));
    }

    public async Task<AttemptResultResponse> FinishAsync(
        Guid attemptUid,
        Guid userUid,
        CancellationToken cancellationToken = default)
    {
        var attempt = await dbContext.Attempts
            .Include(a => a.Answers)
            .Include(a => a.Result)
            .FirstOrDefaultAsync(a => a.Uid == attemptUid, cancellationToken)
            ?? throw new NotFoundException($"Попытка с Uid {attemptUid} не найдена.");

        if (attempt.UserUid != userUid)
        {
            throw new ForbiddenException("Нет доступа к данной попытке.");
        }

        if (attempt.Status == AttemptStatus.Completed || attempt.Result is not null)
        {
            throw new ConflictException("Попытка уже завершена.");
        }

        if (attempt.Status == AttemptStatus.Expired)
        {
            throw new BadRequestException("Нельзя завершить просроченную попытку.");
        }

        // Сначала пробуем получить снапшот из Redis, при промахе — берём из БД
        var snapshot = await snapshotCache.GetAsync(attempt.Uid, cancellationToken) ?? attempt.Snapshot;
        var outcome = gradingService.Grade(snapshot, attempt.Answers.ToList());

        var gradedByQuestion = outcome.Items.ToDictionary(i => i.QuestionUid);
        foreach (var answer in attempt.Answers)
        {
            if (gradedByQuestion.TryGetValue(answer.QuestionUid, out var graded))
            {
                answer.IsCorrect = graded.IsCorrect;
                answer.PointsEarned = graded.PointsEarned;
            }
        }

        var nowUtc = DateTime.UtcNow;

        var result = new Result
        {
            Uid = Guid.NewGuid(),
            AttemptId = attempt.Id,
            TestUid = attempt.TestUid,
            UserUid = attempt.UserUid,
            TotalScore = outcome.TotalScore,
            MaxScore = outcome.MaxScore,
            Percentage = outcome.Percentage,
            Passed = outcome.Passed,
            CalculatedAt = nowUtc
        };

        attempt.Status = AttemptStatus.Completed;
        attempt.FinishedAt = nowUtc;
        attempt.UpdatedAt = nowUtc;
        attempt.Result = result;

        dbContext.Results.Add(result);
        await dbContext.SaveChangesAsync(cancellationToken);

        var completedAtOffset = new DateTimeOffset(nowUtc, TimeSpan.Zero);

        await messagePublisher.PublishAsync(new TestCompletedEvent
        {
            TestUid = attempt.TestUid,
            UserUid = attempt.UserUid,
            AttemptUid = attempt.Uid,
            Score = outcome.TotalScore,
            MaxScore = outcome.MaxScore,
            Percent = outcome.Percentage,
            Passed = outcome.Passed,
            CompletedAt = completedAtOffset
        }, cancellationToken);

        await snapshotCache.RemoveAsync(attempt.Uid, cancellationToken);

        return new AttemptResultResponse(
            ResultUid: result.Uid,
            AttemptUid: attempt.Uid,
            TestUid: attempt.TestUid,
            UserUid: attempt.UserUid,
            Score: result.TotalScore,
            MaxScore: result.MaxScore,
            Percent: result.Percentage,
            PassingScore: snapshot.PassingScore,
            Passed: result.Passed,
            CompletedAt: completedAtOffset,
            QuestionResults: outcome.Items.Select(i => i.ResultDto).ToList());
    }

    public async Task<AttemptResultResponse> GetResultAsync(
        Guid attemptUid,
        Guid userUid,
        CancellationToken cancellationToken = default)
    {
        var attempt = await dbContext.Attempts
            .AsNoTracking()
            .Include(a => a.Answers)
            .Include(a => a.Result)
            .FirstOrDefaultAsync(a => a.Uid == attemptUid, cancellationToken)
            ?? throw new NotFoundException($"Попытка с Uid {attemptUid} не найдена.");

        if (attempt.UserUid != userUid)
        {
            throw new ForbiddenException("Нет доступа к результатам данной попытки.");
        }

        if (attempt.Result is null)
        {
            throw new BadRequestException("Попытка ещё не завершена, результат недоступен.");
        }

        var outcome = gradingService.Grade(attempt.Snapshot, attempt.Answers.ToList());

        return new AttemptResultResponse(
            ResultUid: attempt.Result.Uid,
            AttemptUid: attempt.Uid,
            TestUid: attempt.TestUid,
            UserUid: attempt.UserUid,
            Score: attempt.Result.TotalScore,
            MaxScore: attempt.Result.MaxScore,
            Percent: attempt.Result.Percentage,
            PassingScore: attempt.Snapshot.PassingScore,
            Passed: attempt.Result.Passed,
            CompletedAt: new DateTimeOffset(attempt.Result.CalculatedAt, TimeSpan.Zero),
            QuestionResults: outcome.Items.Select(i => i.ResultDto).ToList());
    }

    private static bool TryMarkExpired(Attempt attempt, DateTime nowUtc)
    {
        if (attempt.Status == AttemptStatus.InProgress
            && attempt.ExpiresAt.HasValue
            && nowUtc > attempt.ExpiresAt.Value)
        {
            attempt.Status = AttemptStatus.Expired;
            attempt.UpdatedAt = nowUtc;
            return true;
        }

        return false;
    }

    private static void ValidateAnswerAgainstQuestion(AttemptQuestionSnapshot question, SubmitAnswerRequest request)
    {
        if (string.Equals(question.QuestionType, "TextInput", StringComparison.OrdinalIgnoreCase))
        {
            if (request.SelectedOptionUids.Count > 0 || string.IsNullOrWhiteSpace(request.TextAnswer))
            {
                throw new BadRequestException("Для данного вопроса требуется текстовый ответ.");
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(request.TextAnswer) || request.SelectedOptionUids.Count == 0)
        {
            throw new BadRequestException("Для данного вопроса необходимо выбрать вариант ответа из списка.");
        }

        if (string.Equals(question.QuestionType, "SingleChoice", StringComparison.OrdinalIgnoreCase)
            && request.SelectedOptionUids.Count != 1)
        {
            throw new BadRequestException("Для вопроса с одиночным выбором можно выбрать только 1 вариант ответа.");
        }

        var validOptionUids = question.Options.Select(o => o.Uid).ToHashSet();
        var invalidOptions = request.SelectedOptionUids.Where(id => !validOptionUids.Contains(id)).ToList();

        if (invalidOptions.Count > 0)
        {
            throw new BadRequestException("Указаны варианты ответа, не относящиеся к данному вопросу.");
        }
    }

    private static AttemptSnapshot MapToInternalSnapshot(TestGradingSnapshot source)
    {
        return new AttemptSnapshot
        {
            TestUid = source.TestUid,
            AuthorUid = source.AuthorUid,
            Title = source.Title,
            Description = source.Description,
            TimeLimitSeconds = source.TimeLimitSeconds,
            MaxAttempts = source.MaxAttempts,
            PassingScore = source.PassingScore,
            StartsAt = source.StartsAt,
            EndsAt = source.EndsAt,
            Questions = source.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new AttemptQuestionSnapshot
                {
                    QuestionUid = q.QuestionUid,
                    Title = q.Title,
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    OrderIndex = q.OrderIndex,
                    Points = q.Points,
                    CorrectTextAnswer = q.CorrectTextAnswer,
                    Options = q.Options
                        .OrderBy(o => o.Position)
                        .Select(o => new AttemptOptionSnapshot
                        {
                            Uid = o.Uid,
                            Text = o.Text,
                            IsCorrect = o.IsCorrect,
                            Position = o.Position
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private static TestSnapshotDto MapToClientSnapshot(AttemptSnapshot snapshot)
    {
        var questions = snapshot.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionSnapshotDto(
                QuestionUid: q.QuestionUid,
                Title: q.Title,
                Text: q.Text,
                QuestionType: q.QuestionType,
                OrderIndex: q.OrderIndex,
                Points: q.Points,
                Options: q.Options
                    .OrderBy(o => o.Position)
                    .Select(o => new AnswerOptionSnapshotDto(o.Uid, o.Text, o.Position))
                    .ToList()))
            .ToList();

        return new TestSnapshotDto(
            TestUid: snapshot.TestUid,
            Title: snapshot.Title,
            Description: snapshot.Description,
            TimeLimitSeconds: snapshot.TimeLimitSeconds,
            PassingScore: snapshot.PassingScore,
            Questions: questions);
    }

    private static AttemptResponse MapToAttemptResponse(Attempt attempt)
    {
        var savedAnswers = attempt.Answers
            .OrderBy(a => a.AnsweredAt)
            .Select(a => new SavedAnswerDto(
                QuestionUid: a.QuestionUid,
                SelectedOptionUids: a.SelectedOptionUids,
                TextAnswer: a.TextAnswer,
                AnsweredAt: new DateTimeOffset(a.AnsweredAt, TimeSpan.Zero)))
            .ToList();

        return new AttemptResponse(
            AttemptUid: attempt.Uid,
            TestUid: attempt.TestUid,
            UserUid: attempt.UserUid,
            Status: attempt.Status.ToString(),
            StartedAt: new DateTimeOffset(attempt.StartedAt, TimeSpan.Zero),
            FinishedAt: attempt.FinishedAt.HasValue
                ? new DateTimeOffset(attempt.FinishedAt.Value, TimeSpan.Zero)
                : null,
            ExpiresAt: attempt.ExpiresAt.HasValue
                ? new DateTimeOffset(attempt.ExpiresAt.Value, TimeSpan.Zero)
                : null,
            TestSnapshot: MapToClientSnapshot(attempt.Snapshot),
            Answers: savedAnswers);
    }
}