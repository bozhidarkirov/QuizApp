using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Web.ViewModels;

namespace QuizApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: /Admin
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserListViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            });
        }

        return View(model);
    }

    // GET: /Admin/EditRoles/{id}
    public async Task<IActionResult> EditRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = allRoles.Select(r => new RoleSelection
            {
                RoleName = r,
                IsSelected = userRoles.Contains(r)
            }).ToList()
        };

        return View(model);
    }

    // POST: /Admin/EditRoles
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
            return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

        // предпазна проверка: не позволявай админ да си махне сам Admin ролята
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == user.Id && currentRoles.Contains("Admin") && !selectedRoles.Contains("Admin"))
        {
            ModelState.AddModelError(string.Empty, "Не можеш да премахнеш собствената си Admin роля.");
            return View(model);
        }

        var rolesToAdd = selectedRoles.Except(currentRoles);
        var rolesToRemove = currentRoles.Except(selectedRoles);

        await _userManager.AddToRolesAsync(user, rolesToAdd);
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        TempData["Success"] = $"Ролите на {user.Email} са обновени успешно.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Admin/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == id)
        {
            TempData["Error"] = "Не можеш да изтриеш собствения си акаунт.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is not null)
        {
            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Потребителят е изтрит.";
        }

        return RedirectToAction(nameof(Index));
    }
}