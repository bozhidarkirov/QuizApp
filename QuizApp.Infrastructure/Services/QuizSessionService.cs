using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;

namespace QuizApp.Infrastructure.Services;

public class QuizSessionService : IQuizSessionService
{
    private readonly ApplicationDbContext _context;
    private static readonly Random _random = new();

    public QuizSessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ---- Генериране на уникален 6-цифрен PIN ----
    private async Task<string> GenerateUniquePinAsync()
    {
        string pin;
        bool exists;

        do
        {
            pin = _random.Next(100000, 999999).ToString();
            exists = await _context.QuizSessions
                .AnyAsync(s => s.PinCode == pin && s.Status != SessionStatus.Finished);
        }
        while (exists);

        return pin;
    }

    public async Task<QuizSession> CreateSessionAsync(int quizId, string hostUserId)
    {
        var pin = await GenerateUniquePinAsync();

        var session = new QuizSession
        {
            QuizId = quizId,
            HostUserId = hostUserId,
            PinCode = pin,
            Status = SessionStatus.Waiting,
            CurrentQuestionIndex = 0
        };

        _context.QuizSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<QuizSession?> GetByIdAsync(int sessionId)
    {
        return await _context.QuizSessions
            .Include(s => s.Quiz)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<QuizSession?> GetByPinAsync(string pin)
    {
        return await _context.QuizSessions
            .Include(s => s.Quiz)
            .FirstOrDefaultAsync(s => s.PinCode == pin && s.Status != SessionStatus.Finished);
    }

    public async Task<QuizSession?> GetWithParticipantsAsync(int sessionId)
    {
        return await _context.QuizSessions
            .Include(s => s.Quiz)
                .ThenInclude(q => q.Questions.OrderBy(qu => qu.OrderIndex))
                    .ThenInclude(qu => qu.Answers)
            .Include(s => s.Participants)
                .ThenInclude(p => p.Responses)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<SessionParticipant> JoinSessionAsync(int sessionId, string nickname, string? userId)
    {
        var participant = new SessionParticipant
        {
            SessionId = sessionId,
            Nickname = nickname,
            UserId = userId,
            TotalScore = 0
        };

        _context.SessionParticipants.Add(participant);
        await _context.SaveChangesAsync();

        return participant;
    }

    public async Task StartSessionAsync(int sessionId)
    {
        var session = await _context.QuizSessions.FindAsync(sessionId);
        if (session is null) return;

        session.Status = SessionStatus.InProgress;
        session.CurrentQuestionIndex = 0;
        session.StartedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<Question?> GetCurrentQuestionAsync(int sessionId)
    {
        var session = await _context.QuizSessions
            .Include(s => s.Quiz)
                .ThenInclude(q => q.Questions.OrderBy(qu => qu.OrderIndex))
                    .ThenInclude(qu => qu.Answers.OrderBy(a => a.OrderIndex))
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null) return null;

        var questions = session.Quiz.Questions.OrderBy(q => q.OrderIndex).ToList();

        if (session.CurrentQuestionIndex >= questions.Count)
            return null;

        return questions[session.CurrentQuestionIndex];
    }

    public async Task<bool> AdvanceToNextQuestionAsync(int sessionId)
    {
        var session = await _context.QuizSessions
            .Include(s => s.Quiz)
                .ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null) return false;

        session.CurrentQuestionIndex++;

        var totalQuestions = session.Quiz.Questions.Count;
        bool hasMore = session.CurrentQuestionIndex < totalQuestions;

        if (!hasMore)
        {
            session.Status = SessionStatus.Finished;
            session.EndedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return hasMore;
    }

    public async Task<SessionResponse> SubmitAnswerAsync(int sessionId, int participantId, int questionId, int? answerId, int responseTimeMs)
{
    var question = await _context.Questions
        .Include(q => q.Answers)
        .FirstOrDefaultAsync(q => q.Id == questionId);

    var participant = await _context.SessionParticipants.FindAsync(participantId);

    if (question is null || participant is null)
        throw new InvalidOperationException("Question or participant not found.");

    var selectedAnswer = answerId.HasValue
        ? question.Answers.FirstOrDefault(a => a.Id == answerId.Value)
        : null;

    bool isCorrect = selectedAnswer?.IsCorrect ?? false;

    // ---- стъпаловидно точкуване по интервали от време ----
    int pointsAwarded = 0;
    if (isCorrect)
    {
        var timeLimitMs = (question.TimeLimitSeconds ?? 20) * 1000;
        var minPoints = (int)Math.Ceiling(question.Points * 0.5);
        var bonusPool = question.Points - minPoints;

        if (bonusPool <= 0)
        {
            pointsAwarded = minPoints;
        }
        else
        {
            var stepLengthMs = (double)timeLimitMs / bonusPool;
            var bucketIndex = (int)Math.Min(bonusPool - 1, responseTimeMs / stepLengthMs);
            var bonusForBucket = bonusPool - bucketIndex;

            pointsAwarded = Math.Max(minPoints, minPoints + bonusForBucket);
        }
    }

    var response = new SessionResponse
    {
        SessionParticipantId = participantId,
        QuestionId = questionId,
        SelectedAnswerId = answerId,
        IsCorrect = isCorrect,
        ResponseTimeMs = responseTimeMs,
        PointsAwarded = pointsAwarded,
        AnsweredAt = DateTime.UtcNow
    };

    _context.SessionResponses.Add(response);

    participant.TotalScore += pointsAwarded;

    await _context.SaveChangesAsync();

    return response;
}

    public async Task<int> GetAnsweredCountAsync(int sessionId, int questionId)
    {
        return await _context.SessionResponses
            .Include(r => r.SessionParticipant)
            .CountAsync(r => r.SessionParticipant!.SessionId == sessionId && r.QuestionId == questionId);
    }

    public async Task<int> GetParticipantCountAsync(int sessionId)
    {
        return await _context.SessionParticipants.CountAsync(p => p.SessionId == sessionId);
    }

    public async Task EndSessionAsync(int sessionId)
    {
        var session = await _context.QuizSessions.FindAsync(sessionId);
        if (session is null) return;

        session.Status = SessionStatus.Finished;
        session.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<List<SessionParticipant>> GetLeaderboardAsync(int sessionId)
    {
        return await _context.SessionParticipants
            .Where(p => p.SessionId == sessionId)
            .OrderByDescending(p => p.TotalScore)
            .ToListAsync();
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
    public async Task<List<QuizSession>> GetSessionHistoryByUserAsync(string userId)
    {
        return await _context.QuizSessions
            .Where(s => s.HostUserId == userId && s.Status == SessionStatus.Finished)
            .Include(s => s.Quiz)
            .Include(s => s.Participants)
            .OrderByDescending(s => s.EndedAt)
            .ToListAsync();
    }
}