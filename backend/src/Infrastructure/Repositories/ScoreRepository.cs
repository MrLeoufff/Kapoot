using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Repositories;

public class ScoreRepository(AppDbContext db) : IScoreRepository
{
    public async Task<Score?> GetByPlayerAndGameSessionAsync(Guid playerId, Guid gameSessionId, CancellationToken cancellationToken = default) =>
        await db.Scores.FirstOrDefaultAsync(x => x.PlayerId == playerId && x.GameSessionId == gameSessionId, cancellationToken);

    public async Task<IReadOnlyList<Score>> GetByGameSessionIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default) =>
        await db.Scores.Where(x => x.GameSessionId == gameSessionId).OrderBy(x => x.Rank).ToListAsync(cancellationToken);

    public async Task<Score> AddAsync(Score score, CancellationToken cancellationToken = default)
    {
        db.Scores.Add(score);
        await db.SaveChangesAsync(cancellationToken);
        return score;
    }

    public async Task UpdateAsync(Score score, CancellationToken cancellationToken = default)
    {
        db.Scores.Update(score);
        await db.SaveChangesAsync(cancellationToken);
    }
}
