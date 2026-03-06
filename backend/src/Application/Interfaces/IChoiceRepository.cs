using Kapoot.Domain.Entities;

namespace Kapoot.Application.Interfaces;

public interface IChoiceRepository
{
    Task<Choice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Choice>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<Choice> AddAsync(Choice choice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Choice choice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
