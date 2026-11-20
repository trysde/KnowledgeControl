using MassTransit;
using SearchService.Services.Interfaces;
using Shared.Contracts.Events;

namespace SearchService.Consumers;

public class TestCompletedConsumer(IStatisticsService statisticsService) : IConsumer<TestCompletedEvent>
{
    public Task Consume(ConsumeContext<TestCompletedEvent> context)
    {
        return statisticsService.OnCompletedAsync(context.Message, context.CancellationToken);
    }
}