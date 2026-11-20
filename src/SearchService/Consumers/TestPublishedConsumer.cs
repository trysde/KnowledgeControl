using MassTransit;
using SearchService.Services.Interfaces;
using Shared.Contracts.Events;

namespace SearchService.Consumers;

public class TestPublishedConsumer(IIndexService indexService) : IConsumer<TestPublishedEvent>
{
    public Task Consume(ConsumeContext<TestPublishedEvent> context)
    {
        return indexService.OnPublishedAsync(context.Message, context.CancellationToken);
    }
}