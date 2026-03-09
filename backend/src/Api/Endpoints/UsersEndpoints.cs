using Kapoot.Application.Queries.GetProfile;
using Microsoft.AspNetCore.Routing;

namespace Kapoot.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{userId:guid}/profile", async (Guid userId, IGetProfileQueryHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetProfileQuery(userId), ct);
            return result.Found ? Results.Ok(result.Profile) : Results.NotFound();
        });

        return app;
    }
}
