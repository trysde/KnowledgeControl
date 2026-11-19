using MassTransit;
using TestCreationService.Services.Interfaces;

namespace TestCreationService.Services.Implementations;

public class RabbitMqMessagePublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        return publishEndpoint.Publish(message, cancellationToken);
    }
}