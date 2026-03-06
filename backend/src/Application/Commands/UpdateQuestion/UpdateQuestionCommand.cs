using Kapoot.Application.Commands.AddQuestion;

namespace Kapoot.Application.Commands.UpdateQuestion;

public record UpdateQuestionCommand(Guid QuestionId, string Text, string? Explanation, int Order, IReadOnlyList<ChoiceInput> Choices);
