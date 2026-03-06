namespace Kapoot.Application.Commands.PublishQuiz;

public interface IPublishQuizCommandHandler
{
    Task HandleAsync(PublishQuizCommand command, CancellationToken cancellationToken = default);
}
