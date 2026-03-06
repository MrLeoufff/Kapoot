namespace Kapoot.Application.Commands.DeleteQuestion;

public interface IDeleteQuestionCommandHandler
{
    Task HandleAsync(DeleteQuestionCommand command, CancellationToken cancellationToken = default);
}
