using Kapoot.Domain.Entities;

namespace Kapoot.Application.Interfaces;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quiz>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quiz>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quiz>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quiz>> GetTopPlayedAsync(int count, CancellationToken cancellationToken = default);
    Task<Quiz> AddAsync(Quiz quiz, CancellationToken cancellationToken = default);
    Task UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
