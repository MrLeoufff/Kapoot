namespace Kapoot.Application.Commands.CreateGameSession;

public interface ICreateGameSessionCommandHandler
{
    Task<CreateGameSessionResult> HandleAsync(CreateGameSessionCommand command, CancellationToken cancellationToken = default);
}
