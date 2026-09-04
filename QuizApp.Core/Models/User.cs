using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
        public ICollection<QuizSession> HostedSessions { get; set; } = new List<QuizSession>();
        public ICollection<SelfPacedAttempt> SelfPacedAttempts { get; set; } = new List<SelfPacedAttempt>();
    }
}
