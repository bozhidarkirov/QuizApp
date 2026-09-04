using Microsoft.AspNetCore.SignalR;
using QuizApp.Core.Interfaces;

namespace QuizApp.Web.Hubs;

public class QuizHub : Hub
{
    private readonly IQuizSessionService _sessionService;

    public QuizHub(IQuizSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    // ---- Учителят се присъединява към своята hub group при отваряне на Host екрана ----
    public async Task JoinHostGroup(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroup(sessionId));
    }

    // ---- Ученик се присъединява към сесия чрез PIN ----
    public async Task JoinSession(int sessionId, string nickname)
    {
        var session = await _sessionService.GetByIdAsync(sessionId);
        if (session is null)
        {
            await Clients.Caller.SendAsync("JoinError", "Сесията не съществува.");
            return;
        }

        var participant = await _sessionService.JoinSessionAsync(sessionId, nickname, null);

        await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroup(sessionId));

        // запазваме participant.Id във връзката, за да го използваме при отговори
        Context.Items["ParticipantId"] = participant.Id;
        Context.Items["SessionId"] = sessionId;

        // уведоми участника за успешно присъединяване (с неговия participantId)
        await Clients.Caller.SendAsync("JoinedSession", participant.Id, participant.Nickname);

        // уведоми всички в групата (включително учителя) за новия участник
        await Clients.Group(GetSessionGroup(sessionId)).SendAsync("ParticipantJoined", participant.Id, participant.Nickname);
    }

    // ---- Учителят стартира играта ----
    public async Task StartGame(int sessionId)
    {
        await _sessionService.StartSessionAsync(sessionId);
        var question = await _sessionService.GetCurrentQuestionAsync(sessionId);

        if (question is null)
        {
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("GameError", "Викторината няма въпроси.");
            return;
        }

        await BroadcastQuestion(sessionId, question);
    }

    // ---- Учителят преминава към следващия въпрос ----
    // ---- Учителят/автоматика преминава към следващия въпрос ----
    public async Task NextQuestion(int sessionId)
    {
        await AdvanceQuestion(sessionId);
    }

    // ---- Ученик изпраща отговор ----
    public async Task SubmitAnswer(int sessionId, int participantId, int questionId, int? answerId, int responseTimeMs)
    {
        var response = await _sessionService.SubmitAnswerAsync(sessionId, participantId, questionId, answerId, responseTimeMs);

        // потвърждение само за самия ученик
        await Clients.Caller.SendAsync("AnswerResult", response.IsCorrect, response.PointsAwarded);

        var answeredCount = await _sessionService.GetAnsweredCountAsync(sessionId, questionId);
        var totalCount = await _sessionService.GetParticipantCountAsync(sessionId);

        await Clients.Group(GetSessionGroup(sessionId)).SendAsync("ParticipantAnswered", answeredCount, totalCount);

        // ---- автоматично преминаване, ако всички са отговорили ----
        if (answeredCount >= totalCount && totalCount > 0)
        {
            await AdvanceQuestion(sessionId);
        }
    }

    // ---- Общ метод за преминаване напред (ползван и от NextQuestion, и от auto-advance) ----
    private async Task AdvanceQuestion(int sessionId)
    {
        var currentQuestion = await _sessionService.GetCurrentQuestionAsync(sessionId);
        if (currentQuestion is not null)
        {
            await _sessionService.RecordMissedAnswersAsync(sessionId, currentQuestion.Id);
        }

        var hasMore = await _sessionService.AdvanceToNextQuestionAsync(sessionId);

        if (!hasMore)
        {
            var leaderboard = await _sessionService.GetLeaderboardAsync(sessionId);
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("GameEnded", leaderboard.Select(p => new
            {
                p.Id,
                p.Nickname,
                p.TotalScore
            }));
            return;
        }

        var question = await _sessionService.GetCurrentQuestionAsync(sessionId);
        if (question is not null)
        {
            await BroadcastQuestion(sessionId, question);
        }
    }

    // ---- Учителят приключва сесията предсрочно ----
    public async Task EndGame(int sessionId)
    {
        await _sessionService.EndSessionAsync(sessionId);
        var leaderboard = await _sessionService.GetLeaderboardAsync(sessionId);

        await Clients.Group(GetSessionGroup(sessionId)).SendAsync("GameEnded", leaderboard.Select(p => new
        {
            p.Id,
            p.Nickname,
            p.TotalScore
        }));
    }

    // ---- Помощен метод: излъчва въпрос към всички в групата, без да разкрива верния отговор ----
    private async Task BroadcastQuestion(int sessionId, Core.Models.Question question)
    {
        var payload = new
        {
            QuestionId = question.Id,
            question.Text,
            question.ImageUrl,
            TimeLimit = question.TimeLimitSeconds ?? 20,
            Answers = question.Answers.OrderBy(a => a.OrderIndex).Select(a => new { a.Id, a.Text })
            // забележка: НЕ изпращаме IsCorrect към клиента, за да не се вижда в browser dev tools
        };

        await Clients.Group(GetSessionGroup(sessionId)).SendAsync("NewQuestion", payload);
    }

    private static string GetSessionGroup(int sessionId) => $"session-{sessionId}";
}