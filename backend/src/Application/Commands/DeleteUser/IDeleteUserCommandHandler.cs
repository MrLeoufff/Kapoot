namespace Kapoot.Application.Commands.DeleteUser;

public interface IDeleteUserCommandHandler
{
    Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default);
}
