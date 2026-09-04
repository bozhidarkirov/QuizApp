using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Models;

namespace QuizApp.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Answer> Answers { get; set; } = null!;
        public DbSet<QuizSession> QuizSessions { get; set; } = null!;
        public DbSet<SessionParticipant> SessionParticipants { get; set; } = null!;
        public DbSet<SessionResponse> SessionResponses { get; set; } = null!;
        public DbSet<SelfPacedAttempt> SelfPacedAttempts { get; set; } = null!;
        public DbSet<SelfPacedAnswer> SelfPacedAnswers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // задължително за Identity таблиците

            // ---- Category -> User (CreatedByUser) ----
            builder.Entity<Category>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Quiz -> Category ----
            builder.Entity<Quiz>()
                .HasOne(q => q.Category)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Quiz -> User (CreatedByUser) ----
            builder.Entity<Quiz>()
                .HasOne(q => q.CreatedByUser)
                .WithMany(u => u.CreatedQuizzes)
                .HasForeignKey(q => q.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Question -> Quiz (тук е ОК cascade, при трие Quiz -> трият се Questions) ----
            builder.Entity<Question>()
                .HasOne(qu => qu.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(qu => qu.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Answer -> Question (cascade ОК) ----
            builder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- QuizSession -> Quiz ----
            builder.Entity<QuizSession>()
                .HasOne(s => s.Quiz)
                .WithMany(q => q.Sessions)
                .HasForeignKey(s => s.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- QuizSession -> User (HostUser) ----
            builder.Entity<QuizSession>()
                .HasOne(s => s.HostUser)
                .WithMany(u => u.HostedSessions)
                .HasForeignKey(s => s.HostUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // уникален PIN код
            builder.Entity<QuizSession>()
                .HasIndex(s => s.PinCode)
                .IsUnique();

            // ---- SessionParticipant -> QuizSession (cascade ОК) ----
            builder.Entity<SessionParticipant>()
                .HasOne(p => p.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- SessionParticipant -> User (nullable, гост опция) ----
            builder.Entity<SessionParticipant>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ---- SessionResponse -> SessionParticipant (cascade ОК) ----
            builder.Entity<SessionResponse>()
                .HasOne(r => r.SessionParticipant)
                .WithMany(p => p.Responses)
                .HasForeignKey(r => r.SessionParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- SessionResponse -> Question ----
            builder.Entity<SessionResponse>()
                .HasOne(r => r.Question)
                .WithMany()
                .HasForeignKey(r => r.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- SessionResponse -> Answer (nullable, ако изтече времето без отговор) ----
            builder.Entity<SessionResponse>()
                .HasOne(r => r.SelectedAnswer)
                .WithMany()
                .HasForeignKey(r => r.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ---- SelfPacedAttempt -> User ----
            builder.Entity<SelfPacedAttempt>()
                .HasOne(a => a.User)
                .WithMany(u => u.SelfPacedAttempts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- SelfPacedAttempt -> Quiz ----
            builder.Entity<SelfPacedAttempt>()
                .HasOne(a => a.Quiz)
                .WithMany(q => q.SelfPacedAttempts)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- SelfPacedAnswer -> SelfPacedAttempt (cascade ОК) ----
            builder.Entity<SelfPacedAnswer>()
                .HasOne(a => a.Attempt)
                .WithMany(at => at.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- SelfPacedAnswer -> Question / Answer ----
            builder.Entity<SelfPacedAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SelfPacedAnswer>()
                .HasOne(a => a.SelectedAnswer)
                .WithMany()
                .HasForeignKey(a => a.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
