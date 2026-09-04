using QuizApp.Core.Models;
using QuizApp.Infrastructure.Services;
using QuizApp.Tests.TestHelpers;
using Xunit;

namespace QuizApp.Tests.Services;

public class QuizSessionServiceTests
{
    [Fact]
    public async Task SubmitAnswerAsync_CorrectAnswer_FastResponse_AwardsMaxPoints()
    {
        using var context = InMemoryDbContextFactory.Create();

        var question = new Question
        {
            Text = "Test question",
            Points = 10,
            TimeLimitSeconds = 20,
            Answers = new List<Answer>
            {
                new Answer { Text = "Correct", IsCorrect = true },
                new Answer { Text = "Wrong", IsCorrect = false }
            }
        };
        context.Questions.Add(question);

        var participant = new SessionParticipant { Nickname = "TestUser" };
        context.SessionParticipants.Add(participant);

        await context.SaveChangesAsync();

        var correctAnswerId = question.Answers.First(a => a.IsCorrect).Id;
        var service = new QuizSessionService(context);

        var response = await service.SubmitAnswerAsync(
            sessionId: 1,
            participantId: participant.Id,
            questionId: question.Id,
            answerId: correctAnswerId,
            responseTimeMs: 1000);

        Assert.True(response.IsCorrect);
        Assert.Equal(10, response.PointsAwarded);
    }

    [Fact]
    public async Task SubmitAnswerAsync_CorrectAnswer_SlowResponse_AwardsMinimumPoints()
    {
        using var context = InMemoryDbContextFactory.Create();

        var question = new Question
        {
            Text = "Test question",
            Points = 10,
            TimeLimitSeconds = 20,
            Answers = new List<Answer>
            {
                new Answer { Text = "Correct", IsCorrect = true },
                new Answer { Text = "Wrong", IsCorrect = false }
            }
        };
        context.Questions.Add(question);

        var participant = new SessionParticipant { Nickname = "TestUser" };
        context.SessionParticipants.Add(participant);

        await context.SaveChangesAsync();

        var correctAnswerId = question.Answers.First(a => a.IsCorrect).Id;
        var service = new QuizSessionService(context);

        var response = await service.SubmitAnswerAsync(
            sessionId: 1,
            participantId: participant.Id,
            questionId: question.Id,
            answerId: correctAnswerId,
            responseTimeMs: 19000);

        Assert.True(response.IsCorrect);
        Assert.Equal(6, response.PointsAwarded);
    }

    [Fact]
    public async Task SubmitAnswerAsync_WrongAnswer_AwardsZeroPoints()
    {
        using var context = InMemoryDbContextFactory.Create();

        var question = new Question
        {
            Text = "Test question",
            Points = 10,
            TimeLimitSeconds = 20,
            Answers = new List<Answer>
            {
                new Answer { Text = "Correct", IsCorrect = true },
                new Answer { Text = "Wrong", IsCorrect = false }
            }
        };
        context.Questions.Add(question);

        var participant = new SessionParticipant { Nickname = "TestUser" };
        context.SessionParticipants.Add(participant);

        await context.SaveChangesAsync();

        var wrongAnswerId = question.Answers.First(a => !a.IsCorrect).Id;
        var service = new QuizSessionService(context);

        var response = await service.SubmitAnswerAsync(
            sessionId: 1,
            participantId: participant.Id,
            questionId: question.Id,
            answerId: wrongAnswerId,
            responseTimeMs: 1000);

        Assert.False(response.IsCorrect);
        Assert.Equal(0, response.PointsAwarded);
    }

    [Fact]
    public async Task SubmitAnswerAsync_NoAnswerSelected_TreatedAsIncorrect()
    {
        using var context = InMemoryDbContextFactory.Create();

        var question = new Question
        {
            Text = "Test question",
            Points = 10,
            TimeLimitSeconds = 20,
            Answers = new List<Answer>
            {
                new Answer { Text = "Correct", IsCorrect = true },
                new Answer { Text = "Wrong", IsCorrect = false }
            }
        };
        context.Questions.Add(question);

        var participant = new SessionParticipant { Nickname = "TestUser" };
        context.SessionParticipants.Add(participant);

        await context.SaveChangesAsync();

        var service = new QuizSessionService(context);

        var response = await service.SubmitAnswerAsync(
            sessionId: 1,
            participantId: participant.Id,
            questionId: question.Id,
            answerId: null,
            responseTimeMs: 20000);

        Assert.False(response.IsCorrect);
        Assert.Equal(0, response.PointsAwarded);
        Assert.Null(response.SelectedAnswerId);
    }
}