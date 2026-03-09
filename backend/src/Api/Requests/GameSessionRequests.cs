namespace Kapoot.Api.Requests;

public record CreateGameSessionRequest(Guid QuizId, Guid HostId);
public record JoinGameSessionRequest(string Code, Guid? UserId, string Pseudo);
