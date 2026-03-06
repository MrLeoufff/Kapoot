namespace Kapoot.Application.Commands.JoinGameSession;

public record JoinGameSessionResult(Guid PlayerId, Guid GameSessionId, string Pseudo);
