using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;

namespace QuizApp.Infrastructure.Services;

public class QuizService : IQuizService
{
    private readonly ApplicationDbContext _context;

    public QuizService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetByIdAsync(int id)
    {
        return await _context.Quizzes.FindAsync(id);
    }

    public async Task<Quiz?> GetWithQuestionsAsync(int id)
    {
        return await _context.Quizzes
            .Include(q => q.Questions.OrderBy(qu => qu.OrderIndex))
                .ThenInclude(qu => qu.Answers.OrderBy(a => a.OrderIndex))
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Quiz>> GetAllByUserAsync(string userId)
    {
        return await _context.Quizzes
            .Where(q => q.CreatedByUserId == userId)
            .Include(q => q.Category)
            .Include(q => q.Questions)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Quiz>> GetPublishedAsync()
    {
        return await _context.Quizzes
            .Where(q => q.IsPublished)
            .Include(q => q.Category)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Quiz> CreateAsync(Quiz quiz)
    {
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }

    public async Task UpdateAsync(Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Sessions)
                .ThenInclude(s => s.Participants)
                    .ThenInclude(p => p.Responses)
            .Include(q => q.SelfPacedAttempts)
                .ThenInclude(a => a.Answers)
            .Include(q => q.Questions)
                .ThenInclude(qu => qu.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz is null)
            return false;

        // ---- каскадно изтриване на всички свързани данни от изиграни сесии ----
        foreach (var session in quiz.Sessions)
        {
            foreach (var participant in session.Participants)
            {
                _context.SessionResponses.RemoveRange(participant.Responses);
            }
            _context.SessionParticipants.RemoveRange(session.Participants);
        }
        _context.QuizSessions.RemoveRange(quiz.Sessions);

        foreach (var attempt in quiz.SelfPacedAttempts)
        {
            _context.SelfPacedAnswers.RemoveRange(attempt.Answers);
        }
        _context.SelfPacedAttempts.RemoveRange(quiz.SelfPacedAttempts);

        // Questions/Answers вече са Cascade delete в модела, ще се изтрият автоматично с Quiz-а

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishAsync(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qu => qu.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz is null || quiz.Questions.Count == 0)
            return false; // не публикуваме викторина без въпроси

        // проверка: всеки въпрос трябва да има поне 2 отговора и точно 1 верен
        foreach (var question in quiz.Questions)
        {
            if (question.Answers.Count < 2 || question.Answers.Count(a => a.IsCorrect) != 1)
                return false;
        }

        quiz.IsPublished = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RecordMissedAnswersAsync(int sessionId, int questionId)
    {
        var participantsWhoAnswered = await _context.SessionResponses
            .Include(r => r.SessionParticipant)
            .Where(r => r.SessionParticipant!.SessionId == sessionId && r.QuestionId == questionId)
            .Select(r => r.SessionParticipantId)
            .ToListAsync();

        var allParticipantIds = await _context.SessionParticipants
            .Where(p => p.SessionId == sessionId)
            .Select(p => p.Id)
            .ToListAsync();

        var missedParticipantIds = allParticipantIds.Except(participantsWhoAnswered).ToList();

        foreach (var participantId in missedParticipantIds)
        {
            _context.SessionResponses.Add(new SessionResponse
            {
                SessionParticipantId = participantId,
                QuestionId = questionId,
                SelectedAnswerId = null,
                IsCorrect = false,
                ResponseTimeMs = 0,
                PointsAwarded = 0,
                AnsweredAt = DateTime.UtcNow
            });
        }

        if (missedParticipantIds.Any())
        {
            await _context.SaveChangesAsync();
        }
    }
    public async Task<List<Quiz>> GetAllAsync()
    {
        return await _context.Quizzes
            .Include(q => q.Category)
            .Include(q => q.Questions)
            .Include(q => q.CreatedByUser)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }
}