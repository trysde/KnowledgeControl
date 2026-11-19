using TestTakingService.Contracts.Attempts;

namespace TestTakingService.Services.Interfaces;

public interface IAttemptService
{
    Task<StartAttemptResponse> StartAsync(Guid testUid, Guid userUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttemptResponse>> GetMyAsync(Guid userUid, CancellationToken cancellationToken = default);
    Task<AttemptResponse> GetByUidAsync(Guid attemptUid, Guid userUid, CancellationToken cancellationToken = default);
    Task<SavedAnswerDto> SubmitAnswerAsync(Guid attemptUid, Guid userUid, SubmitAnswerRequest request, CancellationToken cancellationToken = default);
    Task<AttemptResultResponse> FinishAsync(Guid attemptUid, Guid userUid, CancellationToken cancellationToken = default);
    Task<AttemptResultResponse> GetResultAsync(Guid attemptUid, Guid userUid, CancellationToken cancellationToken = default);
}