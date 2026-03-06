namespace Kapoot.Domain.Entities;

public class Answer
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid QuestionId { get; set; }
    /// <summary>
    /// IDs des choix sélectionnés (QCM multi, Vrai/Faux un seul, etc.)
    /// </summary>
    public ICollection<Guid> SelectedChoiceIds { get; set; } = new List<Guid>();
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
}
