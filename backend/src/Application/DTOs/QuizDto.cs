using Kapoot.Domain.Enums;

namespace Kapoot.Application.DTOs;

public record QuizDto(
    Guid Id,
    string Title,
    string Description,
    QuizStatus Status,
    Guid OwnerId);

