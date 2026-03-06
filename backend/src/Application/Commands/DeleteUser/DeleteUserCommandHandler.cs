using Kapoot.Application.Interfaces;

namespace Kapoot.Application.Commands.DeleteUser;

public class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IQuizRepository quizRepository,
    IGameSessionRepository gameSessionRepository) : IDeleteUserCommandHandler
{
    public async Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new InvalidOperationException("Utilisateur introuvable.");

        var quizzes = await quizRepository.GetByOwnerIdAsync(command.UserId, cancellationToken);
        foreach (var quiz in quizzes)
            await quizRepository.DeleteAsync(quiz.Id, cancellationToken);

        var sessions = await gameSessionRepository.GetByHostIdAsync(command.UserId, cancellationToken);
        foreach (var session in sessions)
            await gameSessionRepository.DeleteAsync(session, cancellationToken);

        await userRepository.DeleteAsync(user, cancellationToken);
    }
}
