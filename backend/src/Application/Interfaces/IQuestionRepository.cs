using Kapoot.Domain.Entities;

namespace Kapoot.Application.Interfaces;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default);
    Task<Question> AddAsync(Question question, CancellationToken cancellationToken = default);
    Task UpdateAsync(Question question, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
