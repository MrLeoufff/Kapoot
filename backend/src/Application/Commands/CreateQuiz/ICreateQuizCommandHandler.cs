namespace Kapoot.Application.Commands.CreateQuiz;

public interface ICreateQuizCommandHandler
{
    Task<CreateQuizResult> HandleAsync(CreateQuizCommand command, CancellationToken cancellationToken = default);
}

