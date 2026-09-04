using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public class Answer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }
}
