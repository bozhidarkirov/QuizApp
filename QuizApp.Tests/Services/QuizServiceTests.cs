using QuizApp.Core.Models;
using QuizApp.Infrastructure.Services;
using QuizApp.Tests.TestHelpers;
using Xunit;

namespace QuizApp.Tests.Services;

public class QuizServiceTests
{
    [Fact]
    public async Task PublishAsync_QuizWithValidQuestions_ReturnsTrue()
    {
        using var context = InMemoryDbContextFactory.Create();

        var quiz = new Quiz
        {
            Title = "Test Quiz",
            CreatedByUserId = "user1",
            Questions = new List<Question>
            {
                new Question
                {
                    Text = "Q1",
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "A", IsCorrect = true },
                        new Answer { Text = "B", IsCorrect = false }
                    }
                }
            }
        };
        context.Quizzes.Add(quiz);
        await context.SaveChangesAsync();

        var service = new QuizService(context);

        var result = await service.PublishAsync(quiz.Id);

        Assert.True(result);

        var updatedQuiz = await context.Quizzes.FindAsync(quiz.Id);
        Assert.True(updatedQuiz!.IsPublished);
    }

    [Fact]
    public async Task PublishAsync_QuizWithNoQuestions_ReturnsFalse()
    {
        using var context = InMemoryDbContextFactory.Create();

        var quiz = new Quiz
        {
            Title = "Empty Quiz",
            CreatedByUserId = "user1"
        };
        context.Quizzes.Add(quiz);
        await context.SaveChangesAsync();

        var service = new QuizService(context);

        var result = await service.PublishAsync(quiz.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task PublishAsync_QuestionWithOnlyOneAnswer_ReturnsFalse()
    {
        using var context = InMemoryDbContextFactory.Create();

        var quiz = new Quiz
        {
            Title = "Invalid Quiz",
            CreatedByUserId = "user1",
            Questions = new List<Question>
            {
                new Question
                {
                    Text = "Q1",
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "A", IsCorrect = true }
                        // само 1 отговор, трябва поне 2
                    }
                }
            }
        };
        context.Quizzes.Add(quiz);
        await context.SaveChangesAsync();

        var service = new QuizService(context);

        var result = await service.PublishAsync(quiz.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task PublishAsync_QuestionWithNoCorrectAnswer_ReturnsFalse()
    {
        using var context = InMemoryDbContextFactory.Create();

        var quiz = new Quiz
        {
            Title = "Invalid Quiz",
            CreatedByUserId = "user1",
            Questions = new List<Question>
            {
                new Question
                {
                    Text = "Q1",
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "A", IsCorrect = false },
                        new Answer { Text = "B", IsCorrect = false }
                        // няма верен отговор
                    }
                }
            }
        };
        context.Quizzes.Add(quiz);
        await context.SaveChangesAsync();

        var service = new QuizService(context);

        var result = await service.PublishAsync(quiz.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task PublishAsync_QuestionWithMultipleCorrectAnswers_ReturnsFalse()
    {
        using var context = InMemoryDbContextFactory.Create();

        var quiz = new Quiz
        {
            Title = "Invalid Quiz",
            CreatedByUserId = "user1",
            Questions = new List<Question>
            {
                new Question
                {
                    Text = "Q1",
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "A", IsCorrect = true },
                        new Answer { Text = "B", IsCorrect = true }
                        // 2 верни отговора, трябва точно 1
                    }
                }
            }
        };
        context.Quizzes.Add(quiz);
        await context.SaveChangesAsync();

        var service = new QuizService(context);

        var result = await service.PublishAsync(quiz.Id);

        Assert.False(result);
    }
}