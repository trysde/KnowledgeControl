using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;
using Shared.Contracts.Events;
using TestCreationService.Contracts.Moderation;
using TestCreationService.Data;
using TestCreationService.Domain.Entities;
using TestCreationService.Domain.Enums;
using TestCreationService.Services.Interfaces;

namespace TestCreationService.Services.Implementations;

public class ModerationService(
    TestCreationDbContext dbContext,
    IMessagePublisher messagePublisher) : IModerationService
{
    public async Task<ReportResponse> ReportAsync(
        Guid reporterUid,
        ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var targetType = Enum.Parse<ModerationTargetType>(request.TargetType, ignoreCase: true);

        var targetExists = targetType switch
        {
            ModerationTargetType.Question => await dbContext.Questions
                .AnyAsync(q => q.Uid == request.TargetUid && !q.IsDeleted, cancellationToken),
            ModerationTargetType.Test => await dbContext.Tests
                .AnyAsync(t => t.Uid == request.TargetUid, cancellationToken),
            _ => false
        };

        if (!targetExists)
        {
            throw new NotFoundException($"Объект модерации ({targetType}) с Uid {request.TargetUid} не найден.");
        }

        var duplicatePending = await dbContext.ModerationReports
            .AnyAsync(r => r.ReporterUid == reporterUid
                        && r.TargetType == targetType
                        && r.TargetUid == request.TargetUid
                        && r.Status == ModerationReportStatus.Pending,
                      cancellationToken);

        if (duplicatePending)
        {
            throw new ConflictException("Вы уже отправили жалобу на этот объект, она находится на рассмотрении.");
        }

        var report = new ModerationReport
        {
            Uid = Guid.NewGuid(),
            ReporterUid = reporterUid,
            TargetType = targetType,
            TargetUid = request.TargetUid,
            Reason = request.Reason.Trim(),
            Status = ModerationReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ModerationReports.Add(report);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ReportResponse(
            Uid: report.Uid,
            ReporterUid: report.ReporterUid,
            TargetType: report.TargetType.ToString(),
            TargetUid: report.TargetUid,
            Reason: report.Reason,
            Status: report.Status.ToString(),
            CreatedAt: new DateTimeOffset(report.CreatedAt, TimeSpan.Zero));
    }

    public async Task HideAsync(
        Guid testUid,
        Guid moderatorUid,
        string? comment = null,
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

        var wasPublished = test.Status == TestStatus.Published && !test.IsHidden;
        var now = DateTime.UtcNow;

        test.IsHidden = true;
        test.Status = TestStatus.Blocked;
        test.UpdatedAt = now;

        var pendingReports = await dbContext.ModerationReports
            .Where(r => r.TargetType == ModerationTargetType.Test
                     && r.TargetUid == testUid
                     && r.Status == ModerationReportStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var report in pendingReports)
        {
            report.Status = ModerationReportStatus.Approved;
            report.ModeratorUid = moderatorUid;
            report.ModeratorComment = string.IsNullOrWhiteSpace(comment)
                ? "Тест скрыт модератором."
                : comment.Trim();
            report.ResolvedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (wasPublished)
        {
            var tags = test.TestQuestions
                .SelectMany(tq => tq.Question.QuestionTags)
                .Select(qt => qt.Tag.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToList();

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
    }
}