using Kapoot.Application.DTOs;
using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Domain.Enums;

namespace Kapoot.Application.Commands.AddQuestion;

public class AddQuestionCommandHandler(
    IQuizRepository quizRepository,
    IQuestionRepository questionRepository,
    IChoiceRepository choiceRepository) : IAddQuestionCommandHandler
{
    public async Task<AddQuestionResult> HandleAsync(AddQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken)
            ?? throw new InvalidOperationException("Quiz introuvable.");
        if (string.IsNullOrWhiteSpace(command.Text))
            throw new ArgumentException("Le texte de la question est obligatoire.", nameof(command.Text));
        if (command.Choices.Count < 2)
            throw new ArgumentException("Au moins deux choix sont requis.", nameof(command.Choices));

        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuizId = command.QuizId,
            Text = command.Text.Trim(),
            Type = command.Type,
            Explanation = command.Explanation?.Trim(),
            Order = command.Order
        };
        await questionRepository.AddAsync(question, cancellationToken);

        var choiceDtos = new List<ChoiceDto>();
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
            choiceDtos.Add(new ChoiceDto(choice.Id, choice.Text, choice.IsCorrect, choice.Order));
        }

        var questionDto = new QuestionDetailDto(question.Id, question.Text, question.Type, question.Explanation, question.Order, choiceDtos);
        return new AddQuestionResult(questionDto);
    }
}
