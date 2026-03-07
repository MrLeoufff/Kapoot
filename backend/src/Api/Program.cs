using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kapoot.Application.Commands.AddQuestion;
using Kapoot.Application.Commands.CreateGameSession;
using Kapoot.Application.Commands.CreateQuiz;
using Kapoot.Application.Commands.DeleteQuestion;
using Kapoot.Application.Commands.JoinGameSession;
using Kapoot.Application.Commands.PublishQuiz;
using Kapoot.Application.Commands.Register;
using Kapoot.Application.Commands.UpdateQuiz;
using Kapoot.Application.Commands.UpdateQuestion;
using Kapoot.Application.Commands.DeleteUser;
using Kapoot.Application.Commands.SetUserAdmin;
using Kapoot.Application.Interfaces;
using Kapoot.Application.Queries.GetProfile;
using Kapoot.Application.Queries.GetQuizDetail;
using Kapoot.Application.Queries.Login;
using Kapoot.Infrastructure.Data;
using Kapoot.Infrastructure.Repositories;
using Kapoot.Domain.Enums;
using Kapoot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// DbContext : MySQL en prod (connection string avec Server=), SQLite sinon
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=kapoot.db";
if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
        options.UseMySql(connectionString, serverVersion);
    });
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Repositories
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IChoiceRepository, ChoiceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();

// Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Application
builder.Services.AddScoped<ICreateQuizCommandHandler, CreateQuizCommandHandler>();
builder.Services.AddScoped<IUpdateQuizCommandHandler, UpdateQuizCommandHandler>();
builder.Services.AddScoped<IPublishQuizCommandHandler, PublishQuizCommandHandler>();
builder.Services.AddScoped<IAddQuestionCommandHandler, AddQuestionCommandHandler>();
builder.Services.AddScoped<IUpdateQuestionCommandHandler, UpdateQuestionCommandHandler>();
builder.Services.AddScoped<IDeleteQuestionCommandHandler, DeleteQuestionCommandHandler>();
builder.Services.AddScoped<ICreateGameSessionCommandHandler, CreateGameSessionCommandHandler>();
builder.Services.AddScoped<IJoinGameSessionCommandHandler, JoinGameSessionCommandHandler>();
builder.Services.AddScoped<IGetQuizDetailQueryHandler, GetQuizDetailQueryHandler>();
builder.Services.AddScoped<IGetProfileQueryHandler, GetProfileQueryHandler>();
builder.Services.AddScoped<IRegisterCommandHandler, RegisterCommandHandler>();
builder.Services.AddScoped<ILoginQueryHandler, LoginQueryHandler>();
builder.Services.AddScoped<IDeleteUserCommandHandler, DeleteUserCommandHandler>();
builder.Services.AddScoped<ISetUserAdminCommandHandler, SetUserAdminCommandHandler>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "KapootSecretKeyMinimum32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "KapootApi";
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => p.RequireClaim("isAdmin", "true"));
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddCors();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

// Créer la base au démarrage (dev) + ajouter colonne IsAdmin si base existante
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    try
    {
        // SQLite et MySQL : ajouter IsAdmin si la table existait déjà sans cette colonne
        db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN IsAdmin INTEGER NOT NULL DEFAULT 0");
    }
    catch
    {
        /* Colonne déjà présente ou erreur SGBD, ignorée */
    }
}


app.UseHttpsRedirection();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<Kapoot.Api.Hubs.GameHub>("/hubs/game");

// --- Auth ---
app.MapPost("/api/auth/register", async (RegisterRequest request, IRegisterCommandHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new RegisterCommand(request.Email, request.Password, request.Pseudo), ct);
        return Results.Created("/api/auth/me", result.User);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("existe déjà"))
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapPost("/api/auth/login", async (LoginRequest request, ILoginQueryHandler handler, IConfiguration config, CancellationToken ct) =>
{
    var result = await handler.HandleAsync(new LoginQuery(request.Email, request.Password), ct);
    if (result is null)
        return Results.Unauthorized();

    var token = GenerateJwt(result.User, config);
    return Results.Ok(new { token, user = result.User });
});

// --- Quiz ---
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

// --- Game sessions ---
app.MapPost("/api/gamesessions", async (CreateGameSessionRequest request, ICreateGameSessionCommandHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new CreateGameSessionCommand(request.QuizId, request.HostId), ct);
        return Results.Created($"/api/gamesessions/{result.SessionId}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/gamesessions/by-code/{code}", async (string code, IGameSessionRepository repository, CancellationToken ct) =>
{
    var session = await repository.GetByCodeAsync(code, ct);
    return session is null ? Results.NotFound() : Results.Ok(session);
});

app.MapGet("/api/gamesessions/by-code/{code}/players", async (string code, IGameSessionRepository repository, IPlayerRepository playerRepository, CancellationToken ct) =>
{
    var session = await repository.GetByCodeAsync(code, ct);
    if (session is null) return Results.NotFound();
    var players = await playerRepository.GetByGameSessionIdAsync(session.Id, ct);
    var list = players.Where(p => !p.HasLeft).Select(p => new { p.Id, p.Pseudo }).ToList();
    return Results.Ok(list);
});

app.MapGet("/api/users/{userId:guid}/profile", async (Guid userId, IGetProfileQueryHandler handler, CancellationToken ct) =>
{
    var result = await handler.HandleAsync(new GetProfileQuery(userId), ct);
    return result.Found ? Results.Ok(result.Profile) : Results.NotFound();
});

app.MapPost("/api/gamesessions/join", async (JoinGameSessionRequest request, IJoinGameSessionCommandHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new JoinGameSessionCommand(request.Code, request.UserId, request.Pseudo), ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// --- Admin (réservé aux utilisateurs avec claim isAdmin) ---
app.MapGet("/api/admin/users", async (IUserRepository userRepo, CancellationToken ct) =>
{
    var users = await userRepo.GetAllAsync(ct);
    var list = users.Select(u => new { u.Id, u.Email, u.Pseudo, u.IsAdmin, u.DateCreated }).ToList();
    return Results.Ok(list);
}).RequireAuthorization("Admin");

app.MapGet("/api/admin/quizzes", async (IQuizRepository quizRepo, CancellationToken ct) =>
{
    var quizzes = await quizRepo.GetAllAsync(ct);
    return Results.Ok(quizzes);
}).RequireAuthorization("Admin");

app.MapDelete("/api/admin/users/{id:guid}", async (Guid id, IDeleteUserCommandHandler handler, CancellationToken ct) =>
{
    try
    {
        await handler.HandleAsync(new DeleteUserCommand(id), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
}).RequireAuthorization("Admin");

app.MapPut("/api/admin/users/{id:guid}/admin", async (Guid id, SetUserAdminRequest request, ISetUserAdminCommandHandler handler, CancellationToken ct) =>
{
    try
    {
        await handler.HandleAsync(new SetUserAdminCommand(id, request.IsAdmin), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
}).RequireAuthorization("Admin");

app.MapDelete("/api/admin/quizzes/{id:guid}", async (Guid id, IQuizRepository quizRepo, IGameSessionRepository sessionRepo, IPlayerRepository playerRepo, IAnswerRepository answerRepo, CancellationToken ct) =>
{
    var quiz = await quizRepo.GetByIdAsync(id, ct);
    if (quiz is null) return Results.NotFound();
    var sessions = await sessionRepo.GetByQuizIdAsync(id, ct);
    var playerIds = new List<Guid>();
    foreach (var session in sessions)
    {
        var players = await playerRepo.GetByGameSessionIdAsync(session.Id, ct);
        playerIds.AddRange(players.Select(p => p.Id));
    }
    await answerRepo.DeleteByPlayerIdsAsync(playerIds, ct);
    foreach (var session in sessions)
        await sessionRepo.DeleteAsync(session, ct);
    await quizRepo.DeleteAsync(id, ct);
    return Results.NoContent();
}).RequireAuthorization("Admin");

app.Run();

static string GenerateJwt(Kapoot.Application.DTOs.UserDto user, IConfiguration config)
{
    var key = config["Jwt:Key"] ?? "KapootSecretKeyMinimum32Characters!";
    var issuer = config["Jwt:Issuer"] ?? "KapootApi";
    var credentials = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        SecurityAlgorithms.HmacSha256);
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
    };
    if (user.IsAdmin)
        claims.Add(new Claim("isAdmin", "true"));
    var token = new JwtSecurityToken(
        issuer,
        issuer,
        claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: credentials);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

public record RegisterRequest(string Email, string Password, string Pseudo);
public record LoginRequest(string Email, string Password);
public record CreateQuizRequest(string Title, string Description, Guid OwnerId);
public record UpdateQuizRequest(string Title, string Description);
public record ChoiceRequest(string Text, bool IsCorrect, int Order);
public record AddQuestionRequest(string Text, int Type, string? Explanation, int Order, List<ChoiceRequest> Choices);
public record UpdateQuestionRequest(string Text, string? Explanation, int Order, List<ChoiceRequest> Choices);
public record CreateGameSessionRequest(Guid QuizId, Guid HostId);
public record JoinGameSessionRequest(string Code, Guid? UserId, string Pseudo);
public record SetUserAdminRequest(bool IsAdmin);

