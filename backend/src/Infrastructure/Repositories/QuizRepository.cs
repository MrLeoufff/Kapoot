using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Repositories;

public class QuizRepository(AppDbContext db) : IQuizRepository
{
    public async Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Quizzes.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Quiz>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Quizzes.OrderByDescending(q => q.UpdatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Quiz>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await db.Quizzes.Where(x => x.OwnerId == ownerId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Quiz>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
        await db.Quizzes.Where(x => x.Status == Domain.Enums.QuizStatus.Published).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Quiz>> GetTopPlayedAsync(int count, CancellationToken cancellationToken = default)
    {
        var topIds = await db.GameSessions.GroupBy(s => s.QuizId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);
        if (topIds.Count == 0)
            return Array.Empty<Quiz>();
        var quizzes = await db.Quizzes.Where(q => topIds.Contains(q.Id)).ToDictionaryAsync(q => q.Id, cancellationToken);
        return topIds.Where(id => quizzes.ContainsKey(id)).Select(id => quizzes[id]).ToList();
    }

    public async Task<Quiz> AddAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(cancellationToken);
        return quiz;
    }

    public async Task UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        db.Quizzes.Update(quiz);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quiz = await db.Quizzes.FindAsync([id], cancellationToken);
        if (quiz is not null)
        {
            db.Quizzes.Remove(quiz);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
