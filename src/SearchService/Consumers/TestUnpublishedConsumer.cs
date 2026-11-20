using MassTransit;
using SearchService.Services.Interfaces;
using Shared.Contracts.Events;

namespace SearchService.Consumers;

public class TestUnpublishedConsumer(IIndexService indexService) : IConsumer<TestUnpublishedEvent>
{
    public Task Consume(ConsumeContext<TestUnpublishedEvent> context)
    {
        return indexService.OnUnpublishedAsync(context.Message, context.CancellationToken);
    }
}