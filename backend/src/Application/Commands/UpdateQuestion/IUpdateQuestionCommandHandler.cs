namespace Kapoot.Application.Commands.UpdateQuestion;

public interface IUpdateQuestionCommandHandler
{
    Task HandleAsync(UpdateQuestionCommand command, CancellationToken cancellationToken = default);
}
