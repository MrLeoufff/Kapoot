namespace Kapoot.Application.Commands.Register;

public interface IRegisterCommandHandler
{
    Task<RegisterResult> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default);
}
