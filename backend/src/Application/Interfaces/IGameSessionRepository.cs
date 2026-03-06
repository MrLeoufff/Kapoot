using Kapoot.Domain.Entities;

namespace Kapoot.Application.Interfaces;

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GameSession?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameSession>> GetByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameSession>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default);
    Task<GameSession> AddAsync(GameSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(GameSession session, CancellationToken cancellationToken = default);
}
