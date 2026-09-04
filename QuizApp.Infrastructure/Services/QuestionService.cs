using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;

namespace QuizApp.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly ApplicationDbContext _context;

    public QuestionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Question?> GetByIdAsync(int id)
    {
        return await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Question>> GetByQuizIdAsync(int quizId)
    {
        return await _context.Questions
            .Where(q => q.QuizId == quizId)
            .Include(q => q.Answers.OrderBy(a => a.OrderIndex))
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<Question> CreateAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task UpdateAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question is not null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderAsync(int quizId, List<int> orderedQuestionIds)
    {
        var questions = await _context.Questions
            .Where(q => q.QuizId == quizId)
            .ToListAsync();

        for (int i = 0; i < orderedQuestionIds.Count; i++)
        {
            var question = questions.FirstOrDefault(q => q.Id == orderedQuestionIds[i]);
            if (question is not null)
                question.OrderIndex = i;
        }

        await _context.SaveChangesAsync();
    }
}