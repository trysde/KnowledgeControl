using MassTransit;
using SearchService.Services.Interfaces;
using Shared.Contracts.Events;

namespace SearchService.Consumers;

public class TestUpdatedConsumer(IIndexService indexService) : IConsumer<TestUpdatedEvent>
{
    public Task Consume(ConsumeContext<TestUpdatedEvent> context)
    {
        return indexService.OnUpdatedAsync(context.Message, context.CancellationToken);
    }
}