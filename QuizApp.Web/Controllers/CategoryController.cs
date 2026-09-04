using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Web.ViewModels;

namespace QuizApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly UserManager<User> _userManager;

    public CategoryController(ICategoryService categoryService, UserManager<User> userManager)
    {
        _categoryService = categoryService;
        _userManager = userManager;
    }

    // GET: /Category
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();

        var model = categories.Select(c => new CategoryListItemViewModel
        {
            Id = c.Id,
            Name = c.Name,
            QuizCount = c.Quizzes.Count
        }).ToList();

        return View(model);
    }

    // GET: /Category/Create
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    // POST: /Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User)!;

        var category = new Category
        {
            Name = model.Name,
            CreatedByUserId = userId
        };

        await _categoryService.CreateAsync(category);

        TempData["Success"] = "Категорията е създадена.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Category/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
            return NotFound();

        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name
        };

        return View(model);
    }

    // POST: /Category/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var category = await _categoryService.GetByIdAsync(model.Id);
        if (category is null)
            return NotFound();

        category.Name = model.Name;
        await _categoryService.UpdateAsync(category);

        TempData["Success"] = "Категорията е обновена.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Category/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _categoryService.DeleteAsync(id);

        if (!success)
            TempData["Error"] = "Категорията не може да бъде изтрита — вероятно има викторини с нея.";
        else
            TempData["Success"] = "Категорията е изтрита.";

        return RedirectToAction(nameof(Index));
    }
}