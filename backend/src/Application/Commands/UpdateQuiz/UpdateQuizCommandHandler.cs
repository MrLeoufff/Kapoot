using Kapoot.Application.Interfaces;

namespace Kapoot.Application.Commands.UpdateQuiz;

public class UpdateQuizCommandHandler(IQuizRepository quizRepository) : IUpdateQuizCommandHandler
{
    public async Task HandleAsync(UpdateQuizCommand command, CancellationToken cancellationToken = default)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken)
            ?? throw new InvalidOperationException("Quiz introuvable.");
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Le titre est obligatoire.", nameof(command.Title));

        quiz.Title = command.Title.Trim();
        quiz.Description = command.Description?.Trim() ?? string.Empty;
        quiz.UpdatedAt = DateTime.UtcNow;
        await quizRepository.UpdateAsync(quiz, cancellationToken);
    }
}
