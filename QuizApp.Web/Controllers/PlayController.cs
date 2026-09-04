using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Web.ViewModels;

namespace QuizApp.Web.Controllers;

[Authorize(Roles = "Student,Teacher,Admin")]
public class PlayController : Controller
{
    private readonly ISelfPacedService _selfPacedService;
    private readonly IQuizService _quizService;
    private readonly UserManager<User> _userManager;

    public PlayController(ISelfPacedService selfPacedService, IQuizService quizService, UserManager<User> userManager)
    {
        _selfPacedService = selfPacedService;
        _quizService = quizService;
        _userManager = userManager;
    }

    // GET: /Play/Start/{quizId}  -- стартира нов опит и показва първия въпрос
    public async Task<IActionResult> Start(int quizId)
    {
        var quiz = await _quizService.GetWithQuestionsAsync(quizId);
        if (quiz is null || !quiz.IsPublished)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var attempt = await _selfPacedService.StartAttemptAsync(quizId, userId);

        return RedirectToAction(nameof(Question), new { attemptId = attempt.Id, index = 0 });
    }

    // GET: /Play/Question/{attemptId}?index=0
    public async Task<IActionResult> Question(int attemptId, int index)
    {
        var attempt = await _selfPacedService.GetAttemptAsync(attemptId);
        if (attempt is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (attempt.UserId != userId)
            return Forbid();

        var questions = attempt.Quiz.Questions.OrderBy(q => q.OrderIndex).ToList();

        if (index >= questions.Count)
        {
            // всички въпроси приключени -> финализирай опита
            await _selfPacedService.CompleteAttemptAsync(attemptId);
            return RedirectToAction(nameof(Summary), new { attemptId });
        }

        var question = questions[index];

        var model = new PlayQuestionViewModel
        {
            AttemptId = attemptId,
            QuestionId = question.Id,
            QuestionText = question.Text,
            QuestionNumber = index + 1,
            TotalQuestions = questions.Count,
            Answers = question.Answers
                .OrderBy(a => a.OrderIndex)
                .Select(a => new PlayAnswerOption { Id = a.Id, Text = a.Text })
                .ToList()
        };

        return View(model);
    }

    // POST: /Play/Answer  -- ученикът избира отговор
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Answer(int attemptId, int questionId, int? answerId, int currentIndex)
    {
        var attempt = await _selfPacedService.GetAttemptAsync(attemptId);
        if (attempt is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (attempt.UserId != userId)
            return Forbid();

        var response = await _selfPacedService.SubmitAnswerAsync(attemptId, questionId, answerId);

        var questions = attempt.Quiz.Questions.OrderBy(q => q.OrderIndex).ToList();
        var totalQuestions = questions.Count;
        var isLastQuestion = (currentIndex + 1) >= totalQuestions;

        var question = questions.First(q => q.Id == questionId);
        var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);

        var model = new PlayResultViewModel
        {
            IsCorrect = response.IsCorrect,
            CorrectAnswerText = correctAnswer?.Text,
            AttemptId = attemptId,
            QuestionNumber = currentIndex + 1,
            TotalQuestions = totalQuestions,
            IsLastQuestion = isLastQuestion
        };

        return View("Result", model);
    }

    // GET: /Play/Next  -- преминава към следващия въпрос (или финализира)
    public IActionResult Next(int attemptId, int currentIndex)
    {
        return RedirectToAction(nameof(Question), new { attemptId, index = currentIndex + 1 });
    }

    // GET: /Play/Summary/{attemptId}
    public async Task<IActionResult> Summary(int attemptId)
    {
        var attempt = await _selfPacedService.GetAttemptAsync(attemptId);
        if (attempt is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (attempt.UserId != userId)
            return Forbid();

        var model = new PlaySummaryViewModel
        {
            QuizTitle = attempt.Quiz.Title,
            Score = attempt.Score,
            TotalQuestions = attempt.TotalQuestions
        };

        return View(model);
    }
}