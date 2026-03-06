using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;

namespace Kapoot.Application.Commands.UpdateQuestion;

public class UpdateQuestionCommandHandler(
    IQuestionRepository questionRepository,
    IChoiceRepository choiceRepository) : IUpdateQuestionCommandHandler
{
    public async Task HandleAsync(UpdateQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var question = await questionRepository.GetByIdAsync(command.QuestionId, cancellationToken)
            ?? throw new InvalidOperationException("Question introuvable.");
        if (string.IsNullOrWhiteSpace(command.Text))
            throw new ArgumentException("Le texte de la question est obligatoire.", nameof(command.Text));

        question.Text = command.Text.Trim();
        question.Explanation = command.Explanation?.Trim();
        question.Order = command.Order;
        await questionRepository.UpdateAsync(question, cancellationToken);

        var existingChoices = await choiceRepository.GetByQuestionIdAsync(question.Id, cancellationToken);
        foreach (var c in existingChoices)
            await choiceRepository.DeleteAsync(c.Id, cancellationToken);

        var order = 0;
        foreach (var c in command.Choices.OrderBy(x => x.Order))
        {
            var choice = new Choice
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Text = c.Text.Trim(),
                IsCorrect = c.IsCorrect,
                Order = order++
            };
            await choiceRepository.AddAsync(choice, cancellationToken);
        }
    }
}
