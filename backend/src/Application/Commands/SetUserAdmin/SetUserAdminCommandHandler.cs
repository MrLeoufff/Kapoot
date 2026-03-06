using Kapoot.Application.Interfaces;

namespace Kapoot.Application.Commands.SetUserAdmin;

public class SetUserAdminCommandHandler(IUserRepository userRepository) : ISetUserAdminCommandHandler
{
    public async Task HandleAsync(SetUserAdminCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new InvalidOperationException("Utilisateur introuvable.");
        user.IsAdmin = command.IsAdmin;
        await userRepository.UpdateAsync(user, cancellationToken);
    }
}
