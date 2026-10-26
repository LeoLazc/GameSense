using System;
using GameSense.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Infrastructure.Data
{
    public class GameSenseDbContext : DbContext
    {
        public GameSenseDbContext(DbContextOptions<GameSenseDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<QuestionCategory> QuestionCategories { get; set; } = null!;
        public DbSet<QuizSession> QuizSessions { get; set; } = null!;
        public DbSet<QuizAnswer> QuizAnswers { get; set; } = null!;
        public DbSet<QuizSessionQuestion> QuizSessionQuestions { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<GotyPrediction> GotyPredictions { get; set; } = null!;
        public DbSet<GotyNominee> GotyNominees { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasAnnotation("GameSense:QuizSeedVersion", 1);

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Username).IsRequired().HasMaxLength(100);
                b.Property(u => u.Email).IsRequired().HasMaxLength(320);
                b.Property(u => u.PasswordHash).IsRequired();
                b.Property(u => u.ExpertiseScore).HasPrecision(5, 2);
                b.HasIndex(u => u.Username).IsUnique();
                b.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Question>(b =>
            {
                b.HasKey(q => q.Id);
                b.Property(q => q.QuestionText).IsRequired().HasMaxLength(1000);
                b.Property(q => q.ExpectedAnswer).IsRequired();
                b.Property(q => q.EvaluationCriteria).IsRequired();
                b.Property(q => q.QuestionWeight).HasPrecision(5, 2);
                b.HasOne(q => q.Category).WithMany(c => c.Questions).HasForeignKey(q => q.CategoryId);
            });

            modelBuilder.Entity<QuestionCategory>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<QuizSession>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Status).IsRequired().HasMaxLength(50);
                b.Property(s => s.FinalScore).HasPrecision(5, 2);
                b.HasOne(s => s.User).WithMany(u => u.QuizSessions).HasForeignKey(s => s.UserId);
                b.HasIndex(s => s.UserId).IsUnique();
            });

            modelBuilder.Entity<QuizAnswer>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.AnswerText).IsRequired();
                b.Property(a => a.AiScore).HasPrecision(5, 2);
                b.Property(a => a.AiConfidence).HasPrecision(5, 2);
                b.Property(a => a.AiEvaluation).IsRequired();
                b.HasOne(a => a.QuizSession).WithMany(s => s.QuizAnswers).HasForeignKey(a => a.QuizSessionId);
                 b.HasOne(a => a.Question).WithMany(q => q.QuizAnswers).HasForeignKey(a => a.QuestionId);
                 b.HasIndex(a => new { a.QuizSessionId, a.QuestionId }).IsUnique();
            });

            modelBuilder.Entity<QuizSessionQuestion>(b =>
            {
                b.HasKey(sq => new { sq.QuizSessionId, sq.QuestionId });
                b.HasIndex(sq => new { sq.QuizSessionId, sq.Order }).IsUnique();
                b.HasOne(sq => sq.QuizSession).WithMany(s => s.QuizSessionQuestions).HasForeignKey(sq => sq.QuizSessionId);
                b.HasOne(sq => sq.Question).WithMany(q => q.QuizSessionQuestions).HasForeignKey(sq => sq.QuestionId);
            });

            modelBuilder.Entity<Review>(b =>
            {
                b.HasKey(r => r.Id);
                b.Property(r => r.RawgGameId).IsRequired().HasMaxLength(50);
                b.Property(r => r.GameTitle).IsRequired().HasMaxLength(200);
                b.Property(r => r.GameImageUrl).HasMaxLength(1000);
                b.Property(r => r.Title).IsRequired().HasMaxLength(200);
                b.Property(r => r.Content).IsRequired().HasMaxLength(5000);
                b.Property(r => r.Rating).IsRequired();
                 b.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
                b.HasIndex(r => new { r.UserId, r.RawgGameId }).IsUnique();
            });

            modelBuilder.Entity<GotyPrediction>(b =>
            {
                b.HasKey(p => p.Id);
                b.HasIndex(p => new { p.UserId, p.Year }).IsUnique();
                b.HasOne(p => p.User).WithMany(u => u.GotyPredictions).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
                b.Property(p => p.GotyRawgGameId).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<GotyNominee>(b =>
            {
                b.HasKey(n => n.Id);
                b.Property(n => n.RawgGameId).IsRequired().HasMaxLength(50);
                b.HasIndex(n => new { n.PredictionId, n.RawgGameId }).IsUnique();
                b.HasIndex(n => new { n.PredictionId, n.Order }).IsUnique();
                b.HasOne(n => n.Prediction).WithMany(p => p.Nominees).HasForeignKey(n => n.PredictionId);
            });
        }
    }
}
