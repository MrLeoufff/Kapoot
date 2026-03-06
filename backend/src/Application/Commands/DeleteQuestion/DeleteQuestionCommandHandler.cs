using Kapoot.Application.Interfaces;

namespace Kapoot.Application.Commands.DeleteQuestion;

public class DeleteQuestionCommandHandler(IQuestionRepository questionRepository) : IDeleteQuestionCommandHandler
{
    public async Task HandleAsync(DeleteQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var question = await questionRepository.GetByIdAsync(command.QuestionId, cancellationToken);
        if (question is not null)
            await questionRepository.DeleteAsync(command.QuestionId, cancellationToken);
    }
}
