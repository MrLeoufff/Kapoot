using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Repositories;

public class QuestionRepository(AppDbContext db) : IQuestionRepository
{
    public async Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Questions.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Question>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default) =>
        await db.Questions.Where(x => x.QuizId == quizId).OrderBy(x => x.Order).ToListAsync(cancellationToken);

    public async Task<Question> AddAsync(Question question, CancellationToken cancellationToken = default)
    {
        db.Questions.Add(question);
        await db.SaveChangesAsync(cancellationToken);
        return question;
    }

    public async Task UpdateAsync(Question question, CancellationToken cancellationToken = default)
    {
        db.Questions.Update(question);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var q = await db.Questions.FindAsync([id], cancellationToken);
        if (q is not null)
        {
            db.Questions.Remove(q);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
