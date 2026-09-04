using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizApp.Core.Models;


namespace QuizApp.Core.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
}