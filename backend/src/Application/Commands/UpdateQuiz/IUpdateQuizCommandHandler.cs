namespace Kapoot.Application.Commands.UpdateQuiz;

public interface IUpdateQuizCommandHandler
{
    Task HandleAsync(UpdateQuizCommand command, CancellationToken cancellationToken = default);
}
