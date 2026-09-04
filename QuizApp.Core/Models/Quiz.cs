using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public User? CreatedByUser { get; set; }

        public int TimePerQuestion { get; set; } = 20; // секунди, default
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizSession> Sessions { get; set; } = new List<QuizSession>();
        public ICollection<SelfPacedAttempt> SelfPacedAttempts { get; set; } = new List<SelfPacedAttempt>();
    }
}
