using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{

    public class SelfPacedAttempt
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SelfPacedAnswer> Answers { get; set; } = new List<SelfPacedAnswer>();
    }
}
