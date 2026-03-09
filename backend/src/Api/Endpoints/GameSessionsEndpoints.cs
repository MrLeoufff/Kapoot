using Kapoot.Api.Requests;
using Kapoot.Application.Commands.CreateGameSession;
using Kapoot.Application.Commands.JoinGameSession;
using Kapoot.Application.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace Kapoot.Api.Endpoints;

public static class GameSessionsEndpoints
{
    public static IEndpointRouteBuilder MapGameSessionsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/gamesessions", async (CreateGameSessionRequest request, ICreateGameSessionCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new CreateGameSessionCommand(request.QuizId, request.HostId), ct);
                return Results.Created($"/api/gamesessions/{result.SessionId}", result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapGet("/api/gamesessions/by-code/{code}", async (string code, IGameSessionRepository repository, CancellationToken ct) =>
        {
            var session = await repository.GetByCodeAsync(code, ct);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        app.MapGet("/api/gamesessions/by-code/{code}/players", async (string code, IGameSessionRepository repository, IPlayerRepository playerRepository, CancellationToken ct) =>
        {
            var session = await repository.GetByCodeAsync(code, ct);
            if (session is null) return Results.NotFound();
            var players = await playerRepository.GetByGameSessionIdAsync(session.Id, ct);
            var list = players.Where(p => !p.HasLeft).Select(p => new { p.Id, p.Pseudo }).ToList();
            return Results.Ok(list);
        });

        app.MapPost("/api/gamesessions/join", async (JoinGameSessionRequest request, IJoinGameSessionCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new JoinGameSessionCommand(request.Code, request.UserId, request.Pseudo), ct);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        return app;
    }
}
