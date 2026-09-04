using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;

namespace QuizApp.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Quizzes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return false;

        if (category.Quizzes.Any())
            return false; // не позволяваме изтриване, ако има викторини с тази категория

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}