using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Repositories;

public class ChoiceRepository(AppDbContext db) : IChoiceRepository
{
    public async Task<Choice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Choices.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Choice>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default) =>
        await db.Choices.Where(x => x.QuestionId == questionId).OrderBy(x => x.Order).ToListAsync(cancellationToken);

    public async Task<Choice> AddAsync(Choice choice, CancellationToken cancellationToken = default)
    {
        db.Choices.Add(choice);
        await db.SaveChangesAsync(cancellationToken);
        return choice;
    }

    public async Task UpdateAsync(Choice choice, CancellationToken cancellationToken = default)
    {
        db.Choices.Update(choice);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = await db.Choices.FindAsync([id], cancellationToken);
        if (c is not null)
        {
            db.Choices.Remove(c);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
