namespace Kapoot.Application.Queries.GetQuizDetail;

public interface IGetQuizDetailQueryHandler
{
    Task<GetQuizDetailResult> HandleAsync(GetQuizDetailQuery query, CancellationToken cancellationToken = default);
}
