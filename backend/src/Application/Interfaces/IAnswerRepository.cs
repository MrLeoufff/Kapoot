using Kapoot.Domain.Entities;

namespace Kapoot.Application.Interfaces;

public interface IAnswerRepository
{
    Task<Answer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Answer>> GetByPlayerAndQuestionAsync(Guid playerId, Guid questionId, CancellationToken cancellationToken = default);
    Task<int> CountCorrectForQuestionInSessionAsync(Guid sessionId, Guid questionId, CancellationToken cancellationToken = default);
    Task<Answer> AddAsync(Answer answer, CancellationToken cancellationToken = default);
}
