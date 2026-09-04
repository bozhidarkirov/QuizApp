using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public class SessionResponse
    {
        public int Id { get; set; }

        public int SessionParticipantId { get; set; }
        public SessionParticipant? SessionParticipant { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int? SelectedAnswerId { get; set; }
        public Answer? SelectedAnswer { get; set; }

        public bool IsCorrect { get; set; }
        public int ResponseTimeMs { get; set; }
        public int PointsAwarded { get; set; }
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
