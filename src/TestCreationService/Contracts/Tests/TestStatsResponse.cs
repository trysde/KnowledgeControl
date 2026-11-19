namespace TestCreationService.Contracts.Tests;

public record TestStatsResponse(
    Guid TestUid,
    string Title,
    string Status,
    int QuestionsCount,
    int TotalPoints,
    int PendingReportsCount);