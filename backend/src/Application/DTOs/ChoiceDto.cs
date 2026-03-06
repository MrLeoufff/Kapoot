namespace Kapoot.Application.DTOs;

public record ChoiceDto(Guid Id, string Text, bool IsCorrect, int Order);
