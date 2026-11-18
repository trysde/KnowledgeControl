namespace Shared.Contracts.Events;

public record UserRegisteredEvent(
    Guid EventId,
    Guid Uid,
    string Nickname,
    DateTimeOffset OccurredAt);