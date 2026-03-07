using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Repositories;

public class AnswerRepository(AppDbContext db) : IAnswerRepository
{
    public async Task<Answer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Answers.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Answer>> GetByPlayerAndQuestionAsync(Guid playerId, Guid questionId, CancellationToken cancellationToken = default) =>
        await db.Answers.Where(x => x.PlayerId == playerId && x.QuestionId == questionId).ToListAsync(cancellationToken);

    public async Task<int> CountCorrectForQuestionInSessionAsync(Guid sessionId, Guid questionId, CancellationToken cancellationToken = default) =>
        await db.Answers
            .Where(a => a.QuestionId == questionId && a.IsCorrect && db.Players.Any(p => p.Id == a.PlayerId && p.GameSessionId == sessionId))
            .CountAsync(cancellationToken);

    public async Task DeleteByPlayerIdsAsync(IEnumerable<Guid> playerIds, CancellationToken cancellationToken = default)
    {
        var ids = playerIds.ToList();
        if (ids.Count == 0) return;
        var toRemove = await db.Answers.Where(a => ids.Contains(a.PlayerId)).ToListAsync(cancellationToken);
        db.Answers.RemoveRange(toRemove);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Answer> AddAsync(Answer answer, CancellationToken cancellationToken = default)
    {
        db.Answers.Add(answer);
        await db.SaveChangesAsync(cancellationToken);
        return answer;
    }
}
