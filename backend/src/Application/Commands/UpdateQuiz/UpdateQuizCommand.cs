namespace Kapoot.Application.Commands.UpdateQuiz;

public record UpdateQuizCommand(Guid QuizId, string Title, string Description);
