using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Web.ViewModels;

namespace QuizApp.Web.Controllers;

[Authorize(Roles = "Student,Teacher,Admin")]
public class StudentController : Controller
{
    private readonly IQuizService _quizService;
    private readonly ISelfPacedService _selfPacedService;
    private readonly UserManager<User> _userManager;

    public StudentController(IQuizService quizService, ISelfPacedService selfPacedService, UserManager<User> userManager)
    {
        _quizService = quizService;
        _selfPacedService = selfPacedService;
        _userManager = userManager;
    }

    // GET: /Student  -- списък с публикувани викторини за самостоятелно решаване
    public async Task<IActionResult> Index()
    {
        var quizzes = await _quizService.GetPublishedAsync();
        var userId = _userManager.GetUserId(User)!;

        var model = new List<StudentQuizListItemViewModel>();

        foreach (var quiz in quizzes)
        {
            var lastAttempt = await _selfPacedService.GetLastAttemptAsync(quiz.Id, userId);

            model.Add(new StudentQuizListItemViewModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CategoryName = quiz.Category?.Name,
                QuestionCount = quiz.Questions.Count,
                LastScore = lastAttempt?.Score,
                LastTotalQuestions = lastAttempt?.TotalQuestions
            });
        }

        return View(model);
    }
}