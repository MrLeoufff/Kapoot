namespace Kapoot.Api.Requests;

public record CreateQuizRequest(string Title, string Description, Guid OwnerId);
public record UpdateQuizRequest(string Title, string Description);
public record ChoiceRequest(string Text, bool IsCorrect, int Order);
public record AddQuestionRequest(string Text, int Type, string? Explanation, int Order, List<ChoiceRequest> Choices);
public record UpdateQuestionRequest(string Text, string? Explanation, int Order, List<ChoiceRequest> Choices);
