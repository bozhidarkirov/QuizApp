using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public class Question
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        public int? TimeLimitSeconds { get; set; } // override на quiz-ниво, ако е null се ползва Quiz.TimePerQuestion
        public int Points { get; set; } = 1000; // базови точки, kahoot-style
        public int OrderIndex { get; set; }

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
