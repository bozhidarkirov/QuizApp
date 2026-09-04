using QuizApp.Core.Models;

namespace QuizApp.Core.Interfaces;

public interface ISelfPacedService
{
    Task<SelfPacedAttempt> StartAttemptAsync(int quizId, string userId);
    Task<SelfPacedAttempt?> GetAttemptAsync(int attemptId);
    Task<SelfPacedAnswer> SubmitAnswerAsync(int attemptId, int questionId, int? answerId);
    Task<SelfPacedAttempt> CompleteAttemptAsync(int attemptId);
    Task<SelfPacedAttempt?> GetLastAttemptAsync(int quizId, string userId);
}