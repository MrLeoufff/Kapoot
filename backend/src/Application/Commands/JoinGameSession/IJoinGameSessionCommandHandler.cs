namespace Kapoot.Application.Commands.JoinGameSession;

public interface IJoinGameSessionCommandHandler
{
    Task<JoinGameSessionResult> HandleAsync(JoinGameSessionCommand command, CancellationToken cancellationToken = default);
}
