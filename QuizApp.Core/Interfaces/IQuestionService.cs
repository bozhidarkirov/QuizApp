using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizApp.Core.Models;

namespace QuizApp.Core.Interfaces;

public interface IQuestionService
{
    Task<Question?> GetByIdAsync(int id);
    Task<List<Question>> GetByQuizIdAsync(int quizId);
    Task<Question> CreateAsync(Question question);
    Task UpdateAsync(Question question);
    Task DeleteAsync(int id);
    Task ReorderAsync(int quizId, List<int> orderedQuestionIds);
}
