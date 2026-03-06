using Kapoot.Domain.Entities;

namespace Kapoot.Application.Interfaces;

public interface IScoreRepository
{
    Task<Score?> GetByPlayerAndGameSessionAsync(Guid playerId, Guid gameSessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Score>> GetByGameSessionIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default);
    Task<Score> AddAsync(Score score, CancellationToken cancellationToken = default);
    Task UpdateAsync(Score score, CancellationToken cancellationToken = default);
}
