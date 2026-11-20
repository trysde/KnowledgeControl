using SearchService.Contracts.Statistics;
using Shared.Contracts.Events;

namespace SearchService.Services.Interfaces;

public interface IStatisticsService
{
    Task OnCompletedAsync(TestCompletedEvent message, CancellationToken cancellationToken = default);
    Task<TestStatisticsResponse?> GetByTestUidAsync(Guid testUid, CancellationToken cancellationToken = default);
}