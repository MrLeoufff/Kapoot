using Kapoot.Api.Endpoints;
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
using Kapoot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Text;

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

// Endpoints par domaine (bonnes pratiques)
app.MapAuthEndpoints();
app.MapQuizzesEndpoints();
app.MapGameSessionsEndpoints();
app.MapUsersEndpoints();
app.MapAdminEndpoints();

app.Run();
