using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Kapoot.Domain.Enums;

namespace Kapoot.Application.Commands.CreateGameSession;

public class CreateGameSessionCommandHandler(IGameSessionRepository gameSessionRepository, IQuizRepository quizRepository) : ICreateGameSessionCommandHandler
{
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 6;

    public async Task<CreateGameSessionResult> HandleAsync(CreateGameSessionCommand command, CancellationToken cancellationToken = default)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken)
            ?? throw new InvalidOperationException("Quiz introuvable.");
        if (quiz.Status != QuizStatus.Published)
            throw new InvalidOperationException("Seuls les quiz publiés peuvent être utilisés pour une partie.");

        var rnd = new Random();
        string code;
        do
        {
            code = GenerateCode(rnd);
        }
        while (await gameSessionRepository.GetByCodeAsync(code, cancellationToken) is not null);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            QuizId = command.QuizId,
            HostId = command.HostId,
            Code = code,
            Status = GameSessionStatus.Waiting,
            CreatedAt = DateTime.UtcNow
        };
        await gameSessionRepository.AddAsync(session, cancellationToken);
        return new CreateGameSessionResult(session.Id, session.Code);
    }

    private static string GenerateCode(Random rnd)
    {
        var span = new char[CodeLength];
        for (var i = 0; i < CodeLength; i++)
            span[i] = CodeChars[rnd.Next(CodeChars.Length)];
        return new string(span);
    }
}
