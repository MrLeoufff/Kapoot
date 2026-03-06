namespace Kapoot.Application.Commands.CreateQuiz;

public record CreateQuizCommand(
    string Title,
    string Description,
    Guid OwnerId);

