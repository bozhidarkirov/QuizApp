// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizApp.Core.Models;
namespace QuizApp.Web.Areas.Identity.Pages.Account.Manage;
public class IndexModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    public IndexModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public string? Username { get; set; }
    [TempData]
    public string? StatusMessage { get; set; }
    [BindProperty]
    public InputModel Input { get; set; } = default!;
    public class InputModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Име")]
        public string? FirstName { get; set; }
    }
    private async Task LoadAsync(User user)
    {
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        Username = isAdmin ? "Админ" : await _userManager.GetUserNameAsync(user);
        Input = new InputModel
        {
            FirstName = user.FirstName
        };
    }
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        await LoadAsync(user);
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        if (Input.FirstName != user.FirstName)
        {
            user.FirstName = Input.FirstName ?? string.Empty;
            await _userManager.UpdateAsync(user);
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }
}