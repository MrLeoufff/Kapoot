using System.Text.Json;
using Kapoot.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

namespace Kapoot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Score> Scores => Set<Score>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Quiz>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne<User>().WithMany((string)null).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Question>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne<Quiz>().WithMany((string)null).HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Choice>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne<Question>().WithMany((string)null).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GameSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne<Quiz>().WithMany((string)null).HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<User>().WithMany((string)null).HasForeignKey(x => x.HostId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne<GameSession>().WithMany((string)null).HasForeignKey(x => x.GameSessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>().WithMany((string)null).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        var guidListComparer = new ValueComparer<ICollection<Guid>>(
            (c1, c2) => (c1 ?? new List<Guid>()).SequenceEqual(c2 ?? new List<Guid>()),
            c => c.Aggregate(0, (a, g) => (a * 31) + g.GetHashCode()),
            c => c.ToList());

        modelBuilder.Entity<Answer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne<Player>().WithMany((string)null).HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Question>().WithMany((string)null).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.SelectedChoiceIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v.ToList(), (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null!) ?? new List<Guid>())
                .Metadata.SetValueComparer(guidListComparer);
        });

        modelBuilder.Entity<Score>(e => e.HasKey(x => x.Id));
        modelBuilder.Entity<Score>().HasOne<Player>().WithMany((string)null).HasForeignKey(s => s.PlayerId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Score>().HasOne<GameSession>().WithMany((string)null).HasForeignKey(s => s.GameSessionId).OnDelete(DeleteBehavior.Cascade);
    }
}
