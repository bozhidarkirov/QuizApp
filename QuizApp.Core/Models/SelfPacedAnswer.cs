using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public class SelfPacedAnswer
    {
        public int Id { get; set; }

        public int AttemptId { get; set; }
        public SelfPacedAttempt? Attempt { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int? SelectedAnswerId { get; set; }
        public Answer? SelectedAnswer { get; set; }

        public bool IsCorrect { get; set; }
    }
}
