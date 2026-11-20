using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SearchService.Contracts.Statistics;
using SearchService.Domain.Entities;
using SearchService.Infrastructure.Persistence;
using SearchService.Services.Interfaces;
using Shared.Contracts.Events;

namespace SearchService.Services.Implementations;

public class StatisticsService(
    SearchDbContext dbContext,
    ISearchCache searchCache,
    ILogger<StatisticsService> logger) : IStatisticsService
{
    public async Task OnCompletedAsync(
        TestCompletedEvent message,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var alreadyProcessed = await dbContext.ProcessedAttempts
            .AnyAsync(p => p.AttemptUid == message.AttemptUid, cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Попытка {AttemptUid} уже была учтена ранее, пропуск дубликата.", message.AttemptUid);
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        dbContext.ProcessedAttempts.Add(new ProcessedAttempt
        {
            AttemptUid = message.AttemptUid,
            ProcessedAt = DateTime.UtcNow
        });

        var stats = await dbContext.TestStatistics
            .FirstOrDefaultAsync(s => s.TestUid == message.TestUid, cancellationToken);

        var completedAtUtc = message.CompletedAt.UtcDateTime;

        if (stats is null)
        {
            var passedAttempts = message.Passed ? 1 : 0;
            stats = new TestStatistics
            {
                TestUid = message.TestUid,
                TotalAttempts = 1,
                PassedAttempts = passedAttempts,
                PassRate = message.Passed ? 100.00m : 0.00m,
                AvgScore = Math.Round(message.Percent, 2, MidpointRounding.AwayFromZero),
                AvgDurationSec = 0,
                LastAttemptAt = completedAtUtc
            };

            dbContext.TestStatistics.Add(stats);
        }
        else
        {
            var previousTotal = stats.TotalAttempts;
            var newTotal = previousTotal + 1;
            var newPassed = stats.PassedAttempts + (message.Passed ? 1 : 0);

            stats.TotalAttempts = newTotal;
            stats.PassedAttempts = newPassed;
            stats.PassRate = Math.Round((decimal)newPassed / newTotal * 100m, 2, MidpointRounding.AwayFromZero);
            stats.AvgScore = Math.Round(
                (stats.AvgScore * previousTotal + message.Percent) / newTotal,
                2,
                MidpointRounding.AwayFromZero);

            if (!stats.LastAttemptAt.HasValue || completedAtUtc > stats.LastAttemptAt.Value)
            {
                stats.LastAttemptAt = completedAtUtc;
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(ex,
                "Конкурентный дубликат при обработке попытки {AttemptUid} для теста {TestUid}.",
                message.AttemptUid, message.TestUid);
            return;
        }

        await searchCache.InvalidateAllAsync(cancellationToken);

        logger.LogInformation(
            "Статистика теста {TestUid} обновлена по попытке {AttemptUid} (всего попыток: {TotalAttempts}).",
            message.TestUid, message.AttemptUid, stats.TotalAttempts);
    }

    public async Task<TestStatisticsResponse?> GetByTestUidAsync(
        Guid testUid,
        CancellationToken cancellationToken = default)
    {
        var stats = await dbContext.TestStatistics
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TestUid == testUid, cancellationToken);

        if (stats is null)
            return null;

        return new TestStatisticsResponse(
            TestUid: stats.TestUid,
            TotalAttempts: stats.TotalAttempts,
            PassedAttempts: stats.PassedAttempts,
            PassRate: stats.PassRate,
            AvgScore: stats.AvgScore,
            AvgDurationSec: stats.AvgDurationSec,
            LastAttemptAt: stats.LastAttemptAt.HasValue
                ? new DateTimeOffset(stats.LastAttemptAt.Value, TimeSpan.Zero)
                : null);
    }
}