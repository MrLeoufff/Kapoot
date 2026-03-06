namespace Kapoot.Application.Queries.GetProfile;

public interface IGetProfileQueryHandler
{
    Task<GetProfileResult> HandleAsync(GetProfileQuery query, CancellationToken cancellationToken = default);
}
