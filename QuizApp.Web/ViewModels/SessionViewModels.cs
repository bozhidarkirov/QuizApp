namespace QuizApp.Web.ViewModels;

public class SessionHistoryItemViewModel
{
    public int Id { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public DateTime? EndedAt { get; set; }
    public int ParticipantCount { get; set; }
}

public class SessionHistoryDetailViewModel
{
    public string QuizTitle { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<LeaderboardEntryViewModel> Leaderboard { get; set; } = new();
}

public class LeaderboardEntryViewModel
{
    public string Nickname { get; set; } = string.Empty;
    public int TotalScore { get; set; }
}