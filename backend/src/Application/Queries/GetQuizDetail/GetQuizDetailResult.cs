using Kapoot.Application.DTOs;

namespace Kapoot.Application.Queries.GetQuizDetail;

public record GetQuizDetailResult(QuizDetailDto? Quiz, bool Found);
