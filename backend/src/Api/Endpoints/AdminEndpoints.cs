using Kapoot.Api.Requests;
using Kapoot.Application.Commands.DeleteUser;
using Kapoot.Application.Commands.SetUserAdmin;
using Kapoot.Application.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace Kapoot.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/users", async (IUserRepository userRepo, CancellationToken ct) =>
        {
            var users = await userRepo.GetAllAsync(ct);
            var list = users.Select(u => new { u.Id, u.Email, u.Pseudo, u.IsAdmin, u.DateCreated }).ToList();
            return Results.Ok(list);
        }).RequireAuthorization("Admin");

        app.MapGet("/api/admin/quizzes", async (IQuizRepository quizRepo, CancellationToken ct) =>
        {
            var quizzes = await quizRepo.GetAllAsync(ct);
            return Results.Ok(quizzes);
        }).RequireAuthorization("Admin");

        app.MapDelete("/api/admin/users/{id:guid}", async (Guid id, IDeleteUserCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                await handler.HandleAsync(new DeleteUserCommand(id), ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization("Admin");

        app.MapPut("/api/admin/users/{id:guid}/admin", async (Guid id, SetUserAdminRequest request, ISetUserAdminCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                await handler.HandleAsync(new SetUserAdminCommand(id, request.IsAdmin), ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization("Admin");

        app.MapDelete("/api/admin/quizzes/{id:guid}", async (Guid id, IQuizRepository quizRepo, IGameSessionRepository sessionRepo, IPlayerRepository playerRepo, IAnswerRepository answerRepo, CancellationToken ct) =>
        {
            var quiz = await quizRepo.GetByIdAsync(id, ct);
            if (quiz is null) return Results.NotFound();
            var sessions = await sessionRepo.GetByQuizIdAsync(id, ct);
            var playerIds = new List<Guid>();
            foreach (var session in sessions)
            {
                var players = await playerRepo.GetByGameSessionIdAsync(session.Id, ct);
                playerIds.AddRange(players.Select(p => p.Id));
            }
            await answerRepo.DeleteByPlayerIdsAsync(playerIds, ct);
            foreach (var session in sessions)
                await sessionRepo.DeleteAsync(session, ct);
            await quizRepo.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        return app;
    }
}
