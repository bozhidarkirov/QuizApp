using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Core.Models
{
    public enum SessionStatus
    {
        Waiting,
        InProgress,
        Finished
    }

    public class QuizSession
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public string HostUserId { get; set; } = string.Empty;
        public User? HostUser { get; set; }

        public string PinCode { get; set; } = string.Empty; // уникален, 6-цифрен
        public SessionStatus Status { get; set; } = SessionStatus.Waiting;
        public int CurrentQuestionIndex { get; set; } = 0;

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
    }
}
