using TestCreationService.Contracts.Questions;

namespace TestCreationService.Services.Interfaces;

public interface IQuestionService
{
    Task<QuestionResponse> CreateAsync(Guid authorUid, CreateQuestionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuestionResponse>> GetMyAsync(Guid authorUid, CancellationToken cancellationToken = default);
    Task<QuestionResponse> GetByIdAsync(Guid questionUid, Guid requesterUid, CancellationToken cancellationToken = default);
    Task<QuestionResponse> UpdateAsync(Guid questionUid, Guid authorUid, UpdateQuestionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid questionUid, Guid authorUid, CancellationToken cancellationToken = default);
}