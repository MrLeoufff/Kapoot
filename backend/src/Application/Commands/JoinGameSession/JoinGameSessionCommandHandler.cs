using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Domain.Enums;

namespace Kapoot.Application.Commands.JoinGameSession;

public class JoinGameSessionCommandHandler(IGameSessionRepository gameSessionRepository, IPlayerRepository playerRepository) : IJoinGameSessionCommandHandler
{
    public async Task<JoinGameSessionResult> HandleAsync(JoinGameSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await gameSessionRepository.GetByCodeAsync(command.Code.Trim().ToUpperInvariant(), cancellationToken)
            ?? throw new InvalidOperationException("Partie introuvable.");
        if (session.Status != GameSessionStatus.Waiting)
            throw new InvalidOperationException("La partie a déjà commencé ou est terminée.");
        if (string.IsNullOrWhiteSpace(command.Pseudo))
            throw new ArgumentException("Le pseudo est obligatoire.", nameof(command.Pseudo));

        var existingPlayers = await playerRepository.GetByGameSessionIdAsync(session.Id, cancellationToken);
        if (existingPlayers.Any(p => p.Pseudo.Equals(command.Pseudo.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ce pseudo est déjà pris dans cette partie.");
        if (command.UserId is { } uid && existingPlayers.Any(p => p.UserId == uid))
            throw new InvalidOperationException("Vous avez déjà rejoint cette partie.");

        var player = new Player
        {
            Id = Guid.NewGuid(),
            GameSessionId = session.Id,
            UserId = command.UserId,
            Pseudo = command.Pseudo.Trim(),
            HasLeft = false
        };
        await playerRepository.AddAsync(player, cancellationToken);
        return new JoinGameSessionResult(player.Id, session.Id, player.Pseudo);
    }
}
