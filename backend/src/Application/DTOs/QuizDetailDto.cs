using Kapoot.Domain.Enums;

namespace Kapoot.Application.DTOs;

public record QuizDetailDto(Guid Id, string Title, string Description, QuizStatus Status, Guid OwnerId, IReadOnlyList<QuestionDetailDto> Questions);
