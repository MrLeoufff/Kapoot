namespace Kapoot.Application.Queries.Login;

public interface ILoginQueryHandler
{
    Task<LoginResult?> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default);
}
