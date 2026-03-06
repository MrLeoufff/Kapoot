using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Repositories;

public class GameSessionRepository(AppDbContext db) : IGameSessionRepository
{
    public async Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.GameSessions.FindAsync([id], cancellationToken);

    public async Task<GameSession?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await db.GameSessions.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public async Task<IReadOnlyList<GameSession>> GetByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default) =>
        await db.GameSessions.Where(x => x.HostId == hostId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<GameSession>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default) =>
        await db.GameSessions.Where(x => x.QuizId == quizId).ToListAsync(cancellationToken);

    public async Task<GameSession> AddAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        db.GameSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        db.GameSessions.Update(session);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        db.GameSessions.Remove(session);
        await db.SaveChangesAsync(cancellationToken);
    }
}
