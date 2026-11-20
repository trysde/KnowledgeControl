using Shared.Contracts.Events;

namespace SearchService.Services.Interfaces;

public interface IIndexService
{
    Task OnPublishedAsync(TestPublishedEvent message, CancellationToken cancellationToken = default);
    Task OnUnpublishedAsync(TestUnpublishedEvent message, CancellationToken cancellationToken = default);
    Task OnUpdatedAsync(TestUpdatedEvent message, CancellationToken cancellationToken = default);
}