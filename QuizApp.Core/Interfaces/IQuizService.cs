using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizApp.Core.Models;

namespace QuizApp.Core.Interfaces;

public interface IQuizService
{
    Task<Quiz?> GetByIdAsync(int id);
    Task<Quiz?> GetWithQuestionsAsync(int id);
    Task<List<Quiz>> GetAllByUserAsync(string userId);
    Task<List<Quiz>> GetPublishedAsync();
    Task<Quiz> CreateAsync(Quiz quiz);
    Task UpdateAsync(Quiz quiz);
    Task<bool> DeleteAsync(int id);
    Task<bool> PublishAsync(int id);

    Task<List<Quiz>> GetAllAsync();
}