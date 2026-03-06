using Kapoot.Application.DTOs;
using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Domain.Enums;

namespace Kapoot.Application.Commands.CreateQuiz;

public class CreateQuizCommandHandler(IQuizRepository quizRepository, IUserRepository userRepository)
    : ICreateQuizCommandHandler
{
    public async Task<CreateQuizResult> HandleAsync(CreateQuizCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Le titre du quiz est obligatoire.", nameof(command.Title));

        // Optionnel : vérifier que le propriétaire existe
        var owner = await userRepository.GetByIdAsync(command.OwnerId, cancellationToken);
        if (owner is null)
            throw new InvalidOperationException("Le propriétaire du quiz n'existe pas.");

        var now = DateTime.UtcNow;

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            Description = command.Description?.Trim() ?? string.Empty,
            Status = QuizStatus.Draft,
            OwnerId = command.OwnerId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var saved = await quizRepository.AddAsync(quiz, cancellationToken);

        var dto = new QuizDto(
            saved.Id,
            saved.Title,
            saved.Description,
            saved.Status,
            saved.OwnerId);

        return new CreateQuizResult(dto);
    }
}

