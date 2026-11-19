namespace Shared.Contracts.Events;

public record TestCompletedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public required Guid TestUid { get; init; }
    public required Guid UserUid { get; init; }
    public required Guid AttemptUid { get; init; }
    public required decimal Score { get; init; }
    public required decimal MaxScore { get; init; }
    public required decimal Percent { get; init; }
    public required bool Passed { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
}