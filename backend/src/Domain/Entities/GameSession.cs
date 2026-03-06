using Kapoot.Domain.Enums;

namespace Kapoot.Domain.Entities;

public class GameSession
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Guid HostId { get; set; }
    public string Code { get; set; } = string.Empty;
    public GameSessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
