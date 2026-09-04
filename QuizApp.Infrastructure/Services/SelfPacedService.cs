using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;

namespace QuizApp.Infrastructure.Services;

public class SelfPacedService : ISelfPacedService
{
    private readonly ApplicationDbContext _context;

    public SelfPacedService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SelfPacedAttempt> StartAttemptAsync(int quizId, string userId)
    {
        var totalQuestions = await _context.Questions.CountAsync(q => q.QuizId == quizId);

        var attempt = new SelfPacedAttempt
        {
            QuizId = quizId,
            UserId = userId,
            Score = 0,
            TotalQuestions = totalQuestions,
            CompletedAt = DateTime.UtcNow // ще се презапише при реално завършване
        };

        _context.SelfPacedAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return attempt;
    }

    public async Task<SelfPacedAttempt?> GetAttemptAsync(int attemptId)
    {
        return await _context.SelfPacedAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions.OrderBy(qu => qu.OrderIndex))
                    .ThenInclude(qu => qu.Answers.OrderBy(ans => ans.OrderIndex))
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId);
    }

    public async Task<SelfPacedAnswer> SubmitAnswerAsync(int attemptId, int questionId, int? answerId)
    {
        var question = await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question is null)
            throw new InvalidOperationException("Question not found.");

        var selectedAnswer = answerId.HasValue
            ? question.Answers.FirstOrDefault(a => a.Id == answerId.Value)
            : null;

        bool isCorrect = selectedAnswer?.IsCorrect ?? false;

        var answer = new SelfPacedAnswer
        {
            AttemptId = attemptId,
            QuestionId = questionId,
            SelectedAnswerId = answerId,
            IsCorrect = isCorrect
        };

        _context.SelfPacedAnswers.Add(answer);

        if (isCorrect)
        {
            var attempt = await _context.SelfPacedAttempts.FindAsync(attemptId);
            if (attempt is not null)
                attempt.Score++;
        }

        await _context.SaveChangesAsync();

        return answer;
    }

    public async Task<SelfPacedAttempt> CompleteAttemptAsync(int attemptId)
    {
        var attempt = await _context.SelfPacedAttempts.FindAsync(attemptId);
        if (attempt is null)
            throw new InvalidOperationException("Attempt not found.");

        attempt.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return attempt;
    }

    public async Task<SelfPacedAttempt?> GetLastAttemptAsync(int quizId, string userId)
    {
        return await _context.SelfPacedAttempts
            .Where(a => a.QuizId == quizId && a.UserId == userId)
            .OrderByDescending(a => a.CompletedAt)
            .FirstOrDefaultAsync();
    }
}