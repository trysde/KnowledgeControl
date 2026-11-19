using TestCreationService.Contracts.Tests;

namespace TestCreationService.Services.Interfaces;

public interface ITestService
{
    Task<TestResponse> CreateAsync(Guid authorUid, CreateTestRequest request, CancellationToken cancellationToken = default);
    Task<TestResponse> UpdateAsync(Guid testUid, Guid authorUid, UpdateTestRequest request, CancellationToken cancellationToken = default);
    Task<PublishTestResponse> PublishAsync(Guid testUid, Guid authorUid, CancellationToken cancellationToken = default);
    Task UnpublishAsync(Guid testUid, Guid authorUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestResponse>> GetMyAsync(Guid authorUid, CancellationToken cancellationToken = default);
    Task<TestSnapshotResponse> GetSnapshotAsync(Guid testUid, bool includeCorrectAnswers = true, CancellationToken cancellationToken = default);
}