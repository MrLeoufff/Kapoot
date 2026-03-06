namespace Kapoot.Application.Commands.AddQuestion;

public interface IAddQuestionCommandHandler
{
    Task<AddQuestionResult> HandleAsync(AddQuestionCommand command, CancellationToken cancellationToken = default);
}
