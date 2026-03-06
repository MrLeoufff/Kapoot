using Kapoot.Application.Interfaces;
using Kapoot.Domain.Enums;

namespace Kapoot.Application.Commands.PublishQuiz;

public class PublishQuizCommandHandler(IQuizRepository quizRepository) : IPublishQuizCommandHandler
{
    public async Task HandleAsync(PublishQuizCommand command, CancellationToken cancellationToken = default)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken)
            ?? throw new InvalidOperationException("Quiz introuvable.");
        quiz.Status = QuizStatus.Published;
        quiz.UpdatedAt = DateTime.UtcNow;
        await quizRepository.UpdateAsync(quiz, cancellationToken);
    }
}
