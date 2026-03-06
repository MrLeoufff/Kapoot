using Kapoot.Domain.Enums;

namespace Kapoot.Application.Commands.AddQuestion;

public record AddQuestionCommand(Guid QuizId, string Text, QuestionType Type, string? Explanation, int Order, IReadOnlyList<ChoiceInput> Choices);
