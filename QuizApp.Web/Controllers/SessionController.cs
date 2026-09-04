using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;

namespace QuizApp.Web.Controllers;

public class SessionController : Controller
{
    private readonly IQuizSessionService _sessionService;
    private readonly IQuizService _quizService;
    private readonly UserManager<User> _userManager;

    public SessionController(IQuizSessionService sessionService, IQuizService quizService, UserManager<User> userManager)
    {
        _sessionService = sessionService;
        _quizService = quizService;
        _userManager = userManager;
    }

    // POST: /Session/Host/{quizId}  -- Teacher стартира нова сесия за своя викторина
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Host(int quizId)
    {
        var quiz = await _quizService.GetByIdAsync(quizId);
        if (quiz is null || !quiz.IsPublished)
        {
            TempData["Error"] = "Викторината трябва да е публикувана, за да стартираш сесия.";
            return RedirectToAction("Edit", "Quiz", new { id = quizId });
        }

        var userId = _userManager.GetUserId(User)!;
        var session = await _sessionService.CreateSessionAsync(quizId, userId);

        return RedirectToAction(nameof(HostLobby), new { sessionId = session.Id });
    }

    // GET: /Session/HostLobby/{sessionId}  -- екран на учителя, чака участници
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> HostLobby(int sessionId)
    {
        var session = await _sessionService.GetWithParticipantsAsync(sessionId);
        if (session is null)
            return NotFound();

        return View(session);
    }

    // GET: /Session/Join  -- Student въвежда PIN
    [AllowAnonymous]
    public IActionResult Join()
    {
        return View();
    }

    // POST: /Session/Join  -- проверка на PIN, пренасочване към Play екрана
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(string pin, string nickname)
    {
        if (string.IsNullOrWhiteSpace(pin) || string.IsNullOrWhiteSpace(nickname))
        {
            ViewBag.Error = "Въведи PIN код и име.";
            return View();
        }

        var session = await _sessionService.GetByPinAsync(pin.Trim());
        if (session is null)
        {
            ViewBag.Error = "Няма сесия с такъв PIN код.";
            return View();
        }

        if (session.Status != Core.Models.SessionStatus.Waiting)
        {
            ViewBag.Error = "Тази сесия вече е започнала или е приключила.";
            return View();
        }

        return RedirectToAction(nameof(Play), new { sessionId = session.Id, nickname });
    }

    // GET: /Session/Play/{sessionId}  -- екран на ученика по време на играта
    [AllowAnonymous]
    public async Task<IActionResult> Play(int sessionId, string nickname)
    {
        var session = await _sessionService.GetByIdAsync(sessionId);
        if (session is null)
            return NotFound();

        ViewBag.Nickname = nickname;
        return View(session);
    }
}