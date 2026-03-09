using Kapoot.Api.Requests;
using Kapoot.Application.Commands.AddQuestion;
using Kapoot.Application.Commands.CreateQuiz;
using Kapoot.Application.Commands.DeleteQuestion;
using Kapoot.Application.Commands.PublishQuiz;
using Kapoot.Application.Commands.UpdateQuestion;
using Kapoot.Application.Commands.UpdateQuiz;
using Kapoot.Application.Queries.GetQuizDetail;
using Kapoot.Application.Interfaces;
using Kapoot.Domain.Enums;
using Microsoft.AspNetCore.Routing;

namespace Kapoot.Api.Endpoints;

public static class QuizzesEndpoints
{
    public static IEndpointRouteBuilder MapQuizzesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/quizzes/{id:guid}", async (Guid id, IQuizRepository repository, CancellationToken ct) =>
        {
            var quiz = await repository.GetByIdAsync(id, ct);
            return quiz is null ? Results.NotFound() : Results.Ok(quiz);
        });

        app.MapGet("/api/quizzes/{id:guid}/detail", async (Guid id, IGetQuizDetailQueryHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetQuizDetailQuery(id), ct);
            return result.Found ? Results.Ok(result.Quiz) : Results.NotFound();
        });

        app.MapGet("/api/quizzes", async (Guid? ownerId, IQuizRepository repository, CancellationToken ct) =>
        {
            if (ownerId is { } id)
                return Results.Ok(await repository.GetByOwnerIdAsync(id, ct));
            return Results.Ok(await repository.GetPublishedAsync(ct));
        }).WithName("GetQuizzes");

        app.MapGet("/api/quizzes/published", async (IQuizRepository repository, CancellationToken ct) =>
            Results.Ok(await repository.GetPublishedAsync(ct)));

        app.MapGet("/api/quizzes/top10", async (IQuizRepository repository, CancellationToken ct) =>
            Results.Ok(await repository.GetTopPlayedAsync(10, ct)));

        app.MapPost("/api/quizzes", async (CreateQuizRequest request, ICreateQuizCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CreateQuizCommand(request.Title, request.Description, request.OwnerId), ct);
            return Results.Created($"/api/quizzes/{result.Quiz.Id}", result.Quiz);
        });

        app.MapPut("/api/quizzes/{id:guid}", async (Guid id, UpdateQuizRequest request, IUpdateQuizCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                await handler.HandleAsync(new UpdateQuizCommand(id, request.Title, request.Description), ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        app.MapPost("/api/quizzes/{id:guid}/publish", async (Guid id, IPublishQuizCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                await handler.HandleAsync(new PublishQuizCommand(id), ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        // --- Questions ---
        app.MapPost("/api/quizzes/{quizId:guid}/questions", async (Guid quizId, AddQuestionRequest request, IAddQuestionCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                var choices = request.Choices.Select(c => new ChoiceInput(c.Text, c.IsCorrect, c.Order)).ToList();
                var result = await handler.HandleAsync(new AddQuestionCommand(quizId, request.Text, (QuestionType)request.Type, request.Explanation, request.Order, choices), ct);
                return Results.Created($"/api/quizzes/{quizId}/questions/{result.Question.Id}", result.Question);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPut("/api/questions/{id:guid}", async (Guid id, UpdateQuestionRequest request, IUpdateQuestionCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                var choices = request.Choices.Select(c => new ChoiceInput(c.Text, c.IsCorrect, c.Order)).ToList();
                await handler.HandleAsync(new UpdateQuestionCommand(id, request.Text, request.Explanation, request.Order, choices), ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        app.MapDelete("/api/questions/{id:guid}", async (Guid id, IDeleteQuestionCommandHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(new DeleteQuestionCommand(id), ct);
            return Results.NoContent();
        });

        return app;
    }
}
