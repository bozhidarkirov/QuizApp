using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public class SessionParticipant
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public QuizSession? Session { get; set; }

        public string? UserId { get; set; } // nullable ако допускаш гости
        public User? User { get; set; }

        public string Nickname { get; set; } = string.Empty;
        public int TotalScore { get; set; } = 0;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SessionResponse> Responses { get; set; } = new List<SessionResponse>();
    }
}
