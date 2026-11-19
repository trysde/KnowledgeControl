using TestCreationService.Contracts.Moderation;

namespace TestCreationService.Services.Interfaces;

public interface IModerationService
{
    Task<ReportResponse> ReportAsync(Guid reporterUid, ReportRequest request, CancellationToken cancellationToken = default);
    Task HideAsync(Guid testUid, Guid moderatorUid, string? comment = null, CancellationToken cancellationToken = default);
}