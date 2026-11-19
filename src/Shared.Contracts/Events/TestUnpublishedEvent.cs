namespace Shared.Contracts.Events;

public record TestUnpublishedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public required Guid TestUid { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required Guid AuthorUid { get; init; }
    public required string Category { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public required DateTimeOffset OccurredAt { get; init; }
}