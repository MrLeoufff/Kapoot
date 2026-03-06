using Kapoot.Domain.Enums;

namespace Kapoot.Application.DTOs;

public record QuestionDetailDto(Guid Id, string Text, QuestionType Type, string? Explanation, int Order, IReadOnlyList<ChoiceDto> Choices);
