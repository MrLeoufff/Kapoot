namespace Kapoot.Application.Commands.JoinGameSession;

public record JoinGameSessionCommand(string Code, Guid? UserId, string Pseudo);
