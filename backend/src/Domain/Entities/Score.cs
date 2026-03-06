namespace Kapoot.Domain.Entities;

public class Score
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid GameSessionId { get; set; }
    public int TotalPoints { get; set; }
    public int? Rank { get; set; }
}
