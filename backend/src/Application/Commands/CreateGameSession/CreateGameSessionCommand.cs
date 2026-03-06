namespace Kapoot.Application.Commands.CreateGameSession;

public record CreateGameSessionCommand(Guid QuizId, Guid HostId);
