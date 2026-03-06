using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Repositories;

public class PlayerRepository(AppDbContext db) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Players.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Player>> GetByGameSessionIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default) =>
        await db.Players.Where(x => x.GameSessionId == gameSessionId && !x.HasLeft).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Player>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await db.Players.Where(x => x.UserId == userId).ToListAsync(cancellationToken);

    public async Task<Player> AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        db.Players.Add(player);
        await db.SaveChangesAsync(cancellationToken);
        return player;
    }

    public async Task UpdateAsync(Player player, CancellationToken cancellationToken = default)
    {
        db.Players.Update(player);
        await db.SaveChangesAsync(cancellationToken);
    }
}
