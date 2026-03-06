using Kapoot.Domain.Entities;

namespace Kapoot.Application.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Player>> GetByGameSessionIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Player>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Player> AddAsync(Player player, CancellationToken cancellationToken = default);
    Task UpdateAsync(Player player, CancellationToken cancellationToken = default);
}
