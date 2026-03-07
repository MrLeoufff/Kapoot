using Kapoot.Api.Requests;
using Kapoot.Api.Services;
using Kapoot.Application.Commands.Register;
using Kapoot.Application.Queries.Login;
using Microsoft.AspNetCore.Routing;

namespace Kapoot.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest request, IRegisterCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new RegisterCommand(request.Email, request.Password, request.Pseudo), ct);
                return Results.Created("/api/auth/me", result.User);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("existe déjà"))
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        app.MapPost("/api/auth/login", async (LoginRequest request, ILoginQueryHandler handler, IConfiguration config, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new LoginQuery(request.Email, request.Password), ct);
            if (result is null)
                return Results.Unauthorized();

            var token = JwtHelper.GenerateJwt(result.User, config);
            return Results.Ok(new { token, user = result.User });
        });

        return app;
    }
}
