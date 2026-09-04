using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizApp.Core.Models;

namespace QuizApp.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        string[] roles = { "Admin", "Teacher", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ---- Създай начален Admin потребител (само ако не съществува) ----
        const string adminEmail = "admin@quizapp.local";
        const string adminPassword = "Admin123!";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    public static async Task SeedCategoriesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        if (!context.Categories.Any())
        {
            var admin = await userManager.FindByEmailAsync("admin@quizapp.local");
            var adminId = admin?.Id ?? string.Empty;

            var categories = new List<Category>
            {
                new Category { Name = "Математика", CreatedByUserId = adminId },
                new Category { Name = "История", CreatedByUserId = adminId },
                new Category { Name = "География", CreatedByUserId = adminId },
                new Category { Name = "Информатика", CreatedByUserId = adminId },
                new Category { Name = "Общи знания", CreatedByUserId = adminId }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}