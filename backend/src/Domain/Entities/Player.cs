namespace Kapoot.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid GameSessionId { get; set; }
    public string Pseudo { get; set; } = string.Empty;
    public bool HasLeft { get; set; }
}
