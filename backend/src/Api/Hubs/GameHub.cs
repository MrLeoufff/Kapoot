using Kapoot.Application.Interfaces;
using Kapoot.Application.Queries.GetQuizDetail;
using Kapoot.Domain.Entities;
using Kapoot.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Kapoot.Api.Hubs;

public class GameHub : Hub
{
    public const string GroupPrefix = "Session_";

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerIdStr = Context.GetHttpContext()?.Request.Query["playerId"].FirstOrDefault();
        if (Guid.TryParse(playerIdStr, out var playerId))
        {
            using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
            var player = await playerRepo.GetByIdAsync(playerId);
            if (player is not null)
            {
                player.HasLeft = true;
                await playerRepo.UpdateAsync(player);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinAsHost(string code)
    {
        using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
        var sessionRepo = scope.ServiceProvider.GetRequiredService<IGameSessionRepository>();
        var session = await sessionRepo.GetByCodeAsync(code.Trim().ToUpperInvariant());
        if (session is null)
            throw new HubException("Partie introuvable.");
        Context.Items["SessionId"] = session.Id;
        Context.Items["Role"] = "Host";
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupPrefix + session.Id);
    }

    public async Task JoinAsPlayer(string code, Guid playerId)
    {
        using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
        var sessionRepo = scope.ServiceProvider.GetRequiredService<IGameSessionRepository>();
        var playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var session = await sessionRepo.GetByCodeAsync(code.Trim().ToUpperInvariant());
        if (session is null)
            throw new HubException("Partie introuvable.");
        var player = await playerRepo.GetByIdAsync(playerId);
        if (player is null)
            throw new HubException("Joueur introuvable. Rejoignez la partie depuis le tableau de bord.");
        if (player.GameSessionId != session.Id)
            throw new HubException("Ce joueur n’appartient pas à cette partie.");
        if (player.HasLeft)
        {
            player.HasLeft = false;
            await playerRepo.UpdateAsync(player);
        }
        Context.Items["SessionId"] = session.Id;
        Context.Items["PlayerId"] = playerId;
        Context.Items["Role"] = "Player";
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupPrefix + session.Id);
    }

    public async Task StartGame()
    {
        var sessionId = (Guid)Context.Items["SessionId"]!;
        if (Context.Items["Role"] as string != "Host")
            throw new HubException("Non autorisé.");
        using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
        var sessionRepo = scope.ServiceProvider.GetRequiredService<IGameSessionRepository>();
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null || session.Status != GameSessionStatus.Waiting)
            throw new HubException("Partie non disponible.");
        session.Status = GameSessionStatus.Running;
        session.StartedAt = DateTime.UtcNow;
        await sessionRepo.UpdateAsync(session);
        await Clients.Group(GroupPrefix + sessionId).SendAsync("GameStarted", new { sessionId, quizId = session.QuizId });
    }

    public async Task ShowQuestion(int questionIndex)
    {
        var sessionId = (Guid)Context.Items["SessionId"]!;
        if (Context.Items["Role"] as string != "Host")
            throw new HubException("Non autorisé.");
        using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
        var sessionRepo = scope.ServiceProvider.GetRequiredService<IGameSessionRepository>();
        var getDetail = scope.ServiceProvider.GetRequiredService<IGetQuizDetailQueryHandler>();
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null)
            throw new HubException("Partie introuvable.");
        var result = await getDetail.HandleAsync(new GetQuizDetailQuery(session.QuizId));
        if (!result.Found || result.Quiz!.Questions.Count <= questionIndex)
            throw new HubException("Question invalide.");
        var q = result.Quiz.Questions[questionIndex];
        var choicesForClient = q.Choices.Select(c => new { c.Id, c.Text, c.Order }).ToList();
        var correctCount = q.Choices.Count(c => c.IsCorrect);
        var allowMultiple = correctCount > 1;
        await Clients.Group(GroupPrefix + sessionId).SendAsync("ShowQuestion", new { questionId = q.Id, q.Text, q.Type, q.Order, choices = choicesForClient, allowMultiple });
    }

    public async Task SubmitAnswer(Guid questionId, List<Guid> choiceIds)
    {
        var sessionId = (Guid)Context.Items["SessionId"]!;
        var playerId = (Guid)Context.Items["PlayerId"]!;
        if (Context.Items["Role"] as string != "Player")
            throw new HubException("Non autorisé.");
        using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
        var answerRepo = scope.ServiceProvider.GetRequiredService<IAnswerRepository>();
        var scoreRepo = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        var choiceRepo = scope.ServiceProvider.GetRequiredService<IChoiceRepository>();
        var playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var questionRepo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();

        var question = await questionRepo.GetByIdAsync(questionId);
        if (question is null)
            throw new HubException("Question introuvable.");
        var correctChoices = (await choiceRepo.GetByQuestionIdAsync(questionId)).Where(c => c.IsCorrect).Select(c => c.Id).ToHashSet();
        var isCorrect = choiceIds.Count == correctChoices.Count && choiceIds.All(correctChoices.Contains);

        var existingCorrectCount = await answerRepo.CountCorrectForQuestionInSessionAsync(sessionId, questionId);
        var rank = isCorrect ? existingCorrectCount + 1 : 0;
        var pointsEarned = isCorrect ? GetPointsForRank(rank) : 0;

        var answer = new Answer
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            QuestionId = questionId,
            SelectedChoiceIds = choiceIds,
            IsCorrect = isCorrect,
            AnsweredAt = DateTime.UtcNow
        };
        await answerRepo.AddAsync(answer);

        var player = await playerRepo.GetByIdAsync(playerId);
        var pseudo = player?.Pseudo ?? "";
        await Clients.Group(GroupPrefix + sessionId).SendAsync("PlayerAnswered", new { playerId, pseudo });
        await Clients.Group(GroupPrefix + sessionId).SendAsync("PointsEarned", new { playerId, pseudo, pointsEarned, rank });

        var score = await scoreRepo.GetByPlayerAndGameSessionAsync(playerId, sessionId);
        if (score is null)
        {
            score = new Score { Id = Guid.NewGuid(), PlayerId = playerId, GameSessionId = sessionId, TotalPoints = pointsEarned };
            await scoreRepo.AddAsync(score);
        }
        else
        {
            score.TotalPoints += pointsEarned;
            await scoreRepo.UpdateAsync(score);
        }

        var scores = await scoreRepo.GetByGameSessionIdAsync(sessionId);
        var ordered = scores.OrderByDescending(s => s.TotalPoints).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Rank = i + 1;
            await scoreRepo.UpdateAsync(ordered[i]);
        }
        var players = await playerRepo.GetByGameSessionIdAsync(sessionId);
        var ranking = ordered.Select(s => new
        {
            s.PlayerId,
            s.TotalPoints,
            s.Rank,
            Pseudo = players.FirstOrDefault(p => p.Id == s.PlayerId)?.Pseudo ?? ""
        }).ToList();
        await Clients.Group(GroupPrefix + sessionId).SendAsync("Ranking", ranking);
    }

    public async Task EndQuestion(Guid questionId, string? explanation)
    {
        var sessionId = (Guid)Context.Items["SessionId"]!;
        if (Context.Items["Role"] as string != "Host")
            throw new HubException("Non autorisé.");
        using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
        var scoreRepo = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        var playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var scores = await scoreRepo.GetByGameSessionIdAsync(sessionId);
        var players = await playerRepo.GetByGameSessionIdAsync(sessionId);
        var ranking = scores.OrderByDescending(s => s.TotalPoints)
            .Select(s => new { s.PlayerId, s.TotalPoints, s.Rank, Pseudo = players.FirstOrDefault(p => p.Id == s.PlayerId)?.Pseudo ?? "" }).ToList();
        await Clients.Group(GroupPrefix + sessionId).SendAsync("ShowResult", new { questionId, explanation });
        await Clients.Group(GroupPrefix + sessionId).SendAsync("Ranking", ranking);
    }

    public async Task EndGame()
    {
        var sessionId = (Guid)Context.Items["SessionId"]!;
        if (Context.Items["Role"] as string != "Host")
            throw new HubException("Non autorisé.");
        using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
        var sessionRepo = scope.ServiceProvider.GetRequiredService<IGameSessionRepository>();
        var scoreRepo = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        var playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is not null)
        {
            session.Status = GameSessionStatus.Finished;
            session.FinishedAt = DateTime.UtcNow;
            await sessionRepo.UpdateAsync(session);
        }
        var scores = await scoreRepo.GetByGameSessionIdAsync(sessionId);
        var players = await playerRepo.GetByGameSessionIdAsync(sessionId);
        var ranking = scores.OrderByDescending(s => s.TotalPoints)
            .Select(s => new { s.PlayerId, s.TotalPoints, s.Rank, Pseudo = players.FirstOrDefault(p => p.Id == s.PlayerId)?.Pseudo ?? "" }).ToList();
        await Clients.Group(GroupPrefix + sessionId).SendAsync("GameEnded", ranking);
    }

    /// <summary>Barème selon l'ordre d'arrivée : 1re = 100, 2e = 80, 3e = 60, 4e = 50, 5e+ = 40.</summary>
    private static int GetPointsForRank(int rank)
    {
        return rank switch
        {
            1 => 100,
            2 => 80,
            3 => 60,
            4 => 50,
            _ => 40
        };
    }
}
