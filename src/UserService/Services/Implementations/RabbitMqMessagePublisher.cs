using MassTransit;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class RabbitMqMessagePublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await publishEndpoint.Publish(message, cancellationToken);
    }
}