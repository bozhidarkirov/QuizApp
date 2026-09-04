using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Web.ViewModels;

namespace QuizApp.Web.Controllers;

[Authorize(Roles = "Teacher,Admin")]
public class QuizController : Controller
{
    private readonly IQuizService _quizService;
    private readonly ICategoryService _categoryService;
    private readonly IQuestionService _questionService;
    private readonly UserManager<User> _userManager;

    public QuizController(
        IQuizService quizService,
        ICategoryService categoryService,
        IQuestionService questionService,
        UserManager<User> userManager)
    {
        _quizService = quizService;
        _categoryService = categoryService;
        _questionService = questionService;
        _userManager = userManager;
    }

    // GET: /Quiz
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var quizzes = await _quizService.GetAllAsync();

        var model = quizzes.Select(q => new QuizListItemViewModel
        {
            Id = q.Id,
            Title = q.Title,
            CategoryName = q.Category?.Name,
            QuestionCount = q.Questions.Count,
            IsPublished = q.IsPublished,
            CreatedAt = q.CreatedAt,
            CreatedByName = q.CreatedByUser?.Email ?? "—",
            IsOwner = q.CreatedByUserId == userId
        }).ToList();

        return View(model);
    }

    // GET: /Quiz/Create
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.GetAllAsync();

        var model = new CreateQuizViewModel
        {
            Categories = categories.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList()
        };

        return View(model);
    }

    // POST: /Quiz/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuizViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllAsync();
            model.Categories = categories.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList();
            return View(model);
        }

        var userId = _userManager.GetUserId(User)!;

        var quiz = new Quiz
        {
            Title = model.Title,
            Description = model.Description,
            CategoryId = model.CategoryId,
            TimePerQuestion = model.TimePerQuestion,
            CreatedByUserId = userId
        };

        var created = await _quizService.CreateAsync(quiz);

        TempData["Success"] = "Викторината е създадена. Сега добави въпроси към нея.";
        return RedirectToAction(nameof(Edit), new { id = created.Id });
    }

    // GET: /Quiz/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await _quizService.GetWithQuestionsAsync(id);
        if (quiz is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (quiz.CreatedByUserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        return View(quiz);
    }

    // POST: /Quiz/Publish/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var success = await _quizService.PublishAsync(id);

        if (!success)
            TempData["Error"] = "Викторината трябва да има поне 1 въпрос, всеки с точно 1 верен отговор.";
        else
            TempData["Success"] = "Викторината е публикувана.";

        return RedirectToAction(nameof(Edit), new { id });
    }

    // POST: /Quiz/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _quizService.GetByIdAsync(id);
        if (quiz is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (quiz.CreatedByUserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        var success = await _quizService.DeleteAsync(id);

        if (!success)
            TempData["Error"] = "Викторината не можа да бъде изтрита.";
        else
            TempData["Success"] = "Викторината е изтрита.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Quiz/AddQuestion/{quizId}
    public async Task<IActionResult> AddQuestion(int quizId)
    {
        var quiz = await _quizService.GetByIdAsync(quizId);
        if (quiz is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (quiz.CreatedByUserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        var model = new CreateQuestionViewModel
        {
            QuizId = quizId,
            TimeLimitSeconds = quiz.TimePerQuestion
        };
        return View(model);
    }

    // POST: /Quiz/AddQuestion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(CreateQuestionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var quiz = await _quizService.GetByIdAsync(model.QuizId);
        if (quiz is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (quiz.CreatedByUserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        var existingQuestions = await _questionService.GetByQuizIdAsync(model.QuizId);
        var nextOrderIndex = existingQuestions.Count;

        var question = new Question
        {
            QuizId = model.QuizId,
            Text = model.Text,
            TimeLimitSeconds = model.TimeLimitSeconds,
            Points = model.Points,
            OrderIndex = nextOrderIndex
        };

        var answerTexts = new[] { model.Answer1, model.Answer2, model.Answer3, model.Answer4 };

        for (int i = 0; i < answerTexts.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(answerTexts[i]))
                continue;

            question.Answers.Add(new Answer
            {
                Text = answerTexts[i]!,
                IsCorrect = (i + 1) == model.CorrectAnswerIndex,
                OrderIndex = i
            });
        }

        await _questionService.CreateAsync(question);

        TempData["Success"] = "Въпросът е добавен успешно.";
        return RedirectToAction(nameof(Edit), new { id = model.QuizId });
    }

    // POST: /Quiz/DeleteQuestion/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, int quizId)
    {
        var question = await _questionService.GetByIdAsync(id);
        if (question is null)
            return NotFound();

        await _questionService.DeleteAsync(id);
        TempData["Success"] = "Въпросът е изтрит.";
        return RedirectToAction(nameof(Edit), new { id = quizId });
    }
}