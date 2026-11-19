namespace TestCreationService.Contracts.Tests;

public record PublishTestResponse(
    Guid Uid,
    string Status,
    DateTimeOffset PublishedAt);