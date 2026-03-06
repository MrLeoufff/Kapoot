using Kapoot.Application.DTOs;
using Kapoot.Application.Interfaces;

namespace Kapoot.Application.Queries.GetQuizDetail;

public class GetQuizDetailQueryHandler(
    IQuizRepository quizRepository,
    IQuestionRepository questionRepository,
    IChoiceRepository choiceRepository) : IGetQuizDetailQueryHandler
{
    public async Task<GetQuizDetailResult> HandleAsync(GetQuizDetailQuery query, CancellationToken cancellationToken = default)
    {
        var quiz = await quizRepository.GetByIdAsync(query.QuizId, cancellationToken);
        if (quiz is null)
            return new GetQuizDetailResult(null, false);

        var questions = await questionRepository.GetByQuizIdAsync(quiz.Id, cancellationToken);
        var questionDtos = new List<QuestionDetailDto>();

        foreach (var q in questions.OrderBy(x => x.Order))
        {
            var choices = await choiceRepository.GetByQuestionIdAsync(q.Id, cancellationToken);
            var choiceDtos = choices.OrderBy(c => c.Order)
                .Select(c => new ChoiceDto(c.Id, c.Text, c.IsCorrect, c.Order))
                .ToList();
            questionDtos.Add(new QuestionDetailDto(q.Id, q.Text, q.Type, q.Explanation, q.Order, choiceDtos));
        }

        var quizDto = new QuizDetailDto(quiz.Id, quiz.Title, quiz.Description, quiz.Status, quiz.OwnerId, questionDtos);
        return new GetQuizDetailResult(quizDto, true);
    }
}
