using QuizApp.Core.Models;

namespace QuizApp.Core.Interfaces;

public interface IQuizSessionService
{
    Task<QuizSession> CreateSessionAsync(int quizId, string hostUserId);
    Task<QuizSession?> GetByIdAsync(int sessionId);
    Task<QuizSession?> GetByPinAsync(string pin);
    Task<QuizSession?> GetWithParticipantsAsync(int sessionId);
    Task<SessionParticipant> JoinSessionAsync(int sessionId, string nickname, string? userId);
    Task StartSessionAsync(int sessionId);
    Task<Question?> GetCurrentQuestionAsync(int sessionId);
    Task<bool> AdvanceToNextQuestionAsync(int sessionId);
    Task<SessionResponse> SubmitAnswerAsync(int sessionId, int participantId, int questionId, int? answerId, int responseTimeMs);
    Task EndSessionAsync(int sessionId);
    Task<List<SessionParticipant>> GetLeaderboardAsync(int sessionId);
    Task<int> GetAnsweredCountAsync(int sessionId, int questionId);
    Task<int> GetParticipantCountAsync(int sessionId);
    Task RecordMissedAnswersAsync(int sessionId, int questionId);

    Task<List<QuizSession>> GetSessionHistoryByUserAsync(string userId);
}