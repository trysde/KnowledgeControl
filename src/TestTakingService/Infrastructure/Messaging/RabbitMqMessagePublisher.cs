using MassTransit;
using TestTakingService.Services.Interfaces;

namespace TestTakingService.Infrastructure.Messaging;

public class RabbitMqMessagePublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        return publishEndpoint.Publish(message, cancellationToken);
    }
}