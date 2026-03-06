namespace Kapoot.Application.Commands.SetUserAdmin;

public interface ISetUserAdminCommandHandler
{
    Task HandleAsync(SetUserAdminCommand command, CancellationToken cancellationToken = default);
}
